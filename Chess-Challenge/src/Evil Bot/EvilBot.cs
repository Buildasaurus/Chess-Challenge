using ChessChallenge.API;
using System;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace ChessChallenge.Example
{
	// A simple bot that can spot mate in one, and always captures the most valuable piece it can.
	// Plays randomly otherwise.
	public class EvilBot : IChessBot
	{
		int counter = 0;

		int[] pieceValues = { 0, 1, 3, 3, 5, 9, 999 };

		public Move Think(Board board, Timer timer)
		{
			Move[] moves = board.GetLegalMoves();
			return bestMove(board, 3, board.IsWhiteToMove);
		}
		static int Abs(int number)
		{
			if (number < 0)
				number = number * -1;

			return number;
		}
		// Test if this move gives checkmate
		bool MoveIsCheckmate(Board board, Move move)
		{
			board.MakeMove(move);
			bool isMate = board.IsInCheckmate();
			board.UndoMove(move);
			return isMate;
		}
		float evaluation(Board board)
		{
			float eval = 0;
			for (int i = 0; i < 64; i++)
			{
				Piece piece = board.GetPiece(new Square(i));
				if (piece.IsWhite)
				{
					eval += pieceValues[(int)piece.PieceType];
					if (piece.IsPawn)
					{
						eval += 0.01f * (piece.Square.Rank - 1);
					}
				}
				else
				{
					eval -= pieceValues[(int)piece.PieceType];
					if (piece.IsPawn)
					{
						eval += 0.01f * (6 - piece.Square.Rank);
					}
				}

			}
			return eval;
		}
		Move bestMove(Board board, int depth, bool playerToMove)
		{
			counter = 0;
			Move bestmove = board.GetLegalMoves()[0];
			float curreval = evaluation(board);
			int sign = playerToMove ? -1 : 1;
			float bestEval = 10000 * sign;
			if (playerToMove) //if white wants the best move
			{
				foreach (Move move in board.GetLegalMoves())
				{
					board.MakeMove(move);
					float eval = minmax(board, depth, -10000, 10000, !playerToMove); //is opposite turn after having done a move
					if (eval > bestEval) //then we should update best move if it is higher
					{
						bestEval = eval;
						bestmove = move;
					}
					board.UndoMove(move);
				}
			}
			else
			{
				foreach (Move move in board.GetLegalMoves())
				{
					board.MakeMove(move);
					float eval = minmax(board, depth, -10000, 10000, !playerToMove); //is opposite turn after having done a move
					if (eval < bestEval)
					{
						bestEval = eval;
						bestmove = move;
					}
					board.UndoMove(move);
				}
			}
			Console.WriteLine("old + " + counter);

			return bestmove;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="board"></param>
		/// <param name="depth"></param>
		/// <param name="alpha"></param>
		/// <param name="beta"></param>
		/// <param name="isMaximizingPlayer">True if it is white to play</param>
		/// <returns></returns>
		float minmax(Board board, int depth, float alpha, float beta, bool isMaximizingPlayer)
		{
			counter++;
			if (board.IsRepeatedPosition() || board.IsDraw())
				return 0;
			if (depth == 0)
			{
				return evaluation(board);
			}
			if (isMaximizingPlayer)
			{
				if (board.IsInCheckmate())
					return -9999;
				float max = -1000;
				foreach (Move move in board.GetLegalMoves())
				{
					board.MakeMove(move);
					float eval = minmax(board, depth - 1, alpha, beta, !isMaximizingPlayer);
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
			else
			{
				if (board.IsInCheckmate())
					return 9999;
				float min = 1000;
				foreach (Move move in board.GetLegalMoves())
				{
					board.MakeMove(move);
					float eval = minmax(board, depth - 1, alpha, beta, !isMaximizingPlayer);
					if (eval < min)
					{
						min = eval;
					}
					board.UndoMove(move);
					beta = Math.Min(beta, min);
					if (beta <= alpha)
					{
						return min;
					}
				}
				return min;
			}
		}
	}
}