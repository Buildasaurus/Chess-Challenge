﻿using ChessChallenge.API;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System;



namespace ChessChallenge.Example
{
	// A simple bot that can spot mate in one, and always captures the most valuable piece it can.
	// Plays randomly otherwise.
	public class EvilBot : IChessBot
	{
		List<int> counters = new List<int>();
		int evalCounter = 0;
		int[,] knightMatrix = {
			{-50, -40, -30, -30, -30, -30, -40, -50},
			{-40, -20, 0, 0, 0, 0, -20, -40},
			{-30, 0, 10, 15, 15, 10, 0, -30},
			{-30, 5, 15, 20, 20, 15, 5,-30},
			{-30, 0, 15, 20, 20, 15, 0,-30},
			{-30, 5, 10, 15, 15, 10, 5, -30},
			{-40, -20, 0, 5, 5, 0, -20, -40},
			{-50, -40, -30, -30, -30, -30, -40, -50}};

		int[,] bishopMatrix =  {
			{-20,-10,-10,-10,-10,-10,-10,-20},
			{-10,  0,  0,  0,  0,  0,  0,-10},
			{-10,  0,  5, 10, 10,  5,  0,-10},
			{-10,  5,  5, 10, 10,  5,  5,-10},
			{-10,  0, 10, 10, 10, 10,  0,-10},
			{-10, 10, 10, 10, 10, 10, 10,-10},
			{-10,  5,  0,  0,  0,  0,  5,-10},
			{-20,-10,-10,-10,-10,-10,-10,-20},
		};
		int[,] kingMatrix = {
			{20, 30, 10, 0, 0, 10, 30, 20},
			{20, 20, -5, -5, -5, -5, 20, 20}};
		int getKingMatrix(int y, int x)
		{
			if (y > 1)
				return -5;
			return kingMatrix[y, x];
		}
		int depth = 3;

		public Move Think(Board board, Timer timer)
		{
			Move[] moves = board.GetLegalMoves();
			/* Can't handle the time - it time outs. even if on so low as 8000 moves, adding two more depth takes it to 800.000
			if(counters.Count > 3)
			{
				int sum = (counters[^1] + counters[^2] + counters[^3]) / 3;
				if (sum < 8000)
					depth += 2;
				if (sum > 500000 && depth > 3)
					depth -= 2;
			}*/
			return bestMove(board, depth, board.IsWhiteToMove);
		}

		int[] pieceValues = { 100, 300, 300, 500, 900, 99999 };


		float evaluation(Board board)
		{
			evalCounter++;
			float eval = 0;
			eval += materialEval(board, true);
			eval -= materialEval(board, false);

			return eval;
		}
		int materialEval(Board board, bool color)
		{
			int eval = 0;
			PieceType[] piectypes = { PieceType.Pawn, PieceType.Knight, PieceType.Bishop, PieceType.Rook,
			PieceType.Queen, PieceType.King};
			for (int i = 0; i < piectypes.Length; i++)
			{
				PieceList pieces = board.GetPieceList(piectypes[i], color);
				eval += pieceValues[i] * pieces.Count;
				foreach (Piece piece in pieces)
				{
					switch (piece.PieceType)
					{
						case PieceType.Pawn:
							eval += (piece.IsWhite ? piece.Square.Rank - 1 : 6 - piece.Square.Rank);
							break;
						case PieceType.Knight:
							eval += knightMatrix[piece.Square.Rank, piece.Square.File];
							break;
						case PieceType.Bishop:
							eval += bishopMatrix[piece.Square.Rank, piece.Square.File];
							break;
						case PieceType.King:
							if (board.GameMoveHistory.Length < 50)
								eval += getKingMatrix(piece.IsWhite ? piece.Square.Rank : 7 - piece.Square.Rank, piece.Square.File);
							break;
					}
				}
			}
			return eval;
		}


		Move bestMove(Board board, int depth, bool playerToMove)
		{
			counters.Add(0);
			evalCounter = 0;
			Move bestmove = board.GetLegalMoves()[0];
			int color = playerToMove ? 1 : -1;
			float bestEval = -1000000;
			Move[] legalmoves = board.GetLegalMoves();
			Array.Sort(legalmoves, (a, b) => MoveOrderingHeuristic(b, board).CompareTo(MoveOrderingHeuristic(a, board)));

			foreach (Move move in legalmoves)
			{
				board.MakeMove(move);
				float eval = -negamax(board, depth, -1000000, 1000000, -color);
				if (eval > bestEval)
				{
					bestEval = eval;
					bestmove = move;
				}
				board.UndoMove(move);
			}

			Console.WriteLine(counters[^1]);
			Console.WriteLine(evalCounter);
			return bestmove;
		}

		int MoveOrderingHeuristic(Move move, Board board)
		{
			int score = 0;

			// Give higher scores to captures and promotions
			if (move.IsCapture)
			{
				// Use MVV-LVA heuristic
				score += 1000 * (pieceValues[(int)move.CapturePieceType - 1] - pieceValues[(int)move.MovePieceType - 1]);
			}
			if (move.IsPromotion)
				score += 500;

			// Give a small bonus for checks
			board.MakeMove(move);
			if (board.IsInCheck())
				score += 10;
			board.UndoMove(move);

			return score;
		}

		/*
		int StaticExchangeEvaluation(Move move, Board board)
		{
			int score = 0;
			int attackerValue = pieceValues[(int)board.GetPiece(move.StartSquare).PieceType];
			int victimValue = pieceValues[(int)board.GetPiece(move.TargetSquare).PieceType];

			// Make the move on the board
			board.MakeMove(move);

			// Calculate the gain from capturing the victim
			int gain = victimValue - attackerValue;

			// Find the least valuable attacker of the opposite color
			Move bestCapture = FindLeastValuableAttacker(move.TargetSquare, board);

			// If there is a capture, recursively evaluate it
			if (bestCapture != Move.NullMove)
				gain -= StaticExchangeEvaluation(bestCapture, board);

			// Undo the move on the board
			board.UndoMove(move);

			// The score is the maximum of 0 and the gain
			score = Math.Max(0, gain);

			return score;
		}

		Move FindLeastValuableAttacker(Square square, Board board)
		{
			Move bestCapture = Move.NullMove;
			int bestValue = 9999999;

			// Iterate over all pieces of the opposite color
			foreach (Move move in board.GetLegalMoves(true))
			{
				// Check if the piece can capture the square
				if (move.TargetSquare == square)
				{
					// Check if the piece is less valuable than the current best
					int value = pieceValues[(int)board.GetPiece(square).PieceType];
					if (value < bestValue)
					{
						// Update the best capture and value
						bestCapture = move;
						bestValue = value;
					}
				}
			}
			return bestCapture;
		}*/



		/// <summary>
		/// 
		/// </summary>
		/// <param name="board"></param>
		/// <param name="depth"></param>
		/// <param name="alpha"></param>
		/// <param name="beta"></param>
		/// <param name="isMaximizingPlayer">True if it is white to play</param>
		/// <returns></returns>
		float negamax(Board board, int depth, float alpha, float beta, int color)
		{
			if (board.IsInCheckmate())
				return -999999;
			counters[^1]++;
			if (board.IsRepeatedPosition() || board.IsDraw())
				return 0;
			if (depth == 0)
			{
				return color * evaluation(board);
			}

			float max = -100000;
			foreach (Move move in board.GetLegalMoves())
			{
				board.MakeMove(move);
				float eval = -negamax(board, depth - 1, -beta, -alpha, -color);
				if (eval > max)
				{
					max = eval;
				}
				board.UndoMove(move);
				alpha = Math.Max(alpha, max);
				if (beta <= alpha)
				{
					return max;
				}
			}
			return max;
		}
	}
}