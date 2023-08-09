using ChessChallenge.API;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System;


/// <summary>
/// 
/// Ideas
/// - Kind bot - When winning - go for a draw instead 
/// - No queen bot - just throw it away and try to win anyways
/// - scholars mate / trick bot - always go for certain tricks.
/// </summary>
public class MyBot : IChessBot
{
	int counter = 0;
	int evalCounter = 0;
	int[,] knightMatrix = {
		{0, -5, 1, 1, 1, 1, -5, 0},
		{0, 4, 5, 5, 5, 5, 4, 0},
		{1, 5, 7, 7, 7, 7, 5, 1},
		{1, 5, 7, 8, 8, 7, 5 ,1},
		{1 ,5 ,7 ,8 ,8 ,7 ,5 ,1},
		{1 ,5 ,7 ,7 ,7 ,7 ,5 ,1},
		{1 ,4 ,5 ,5 ,5 ,5 ,4 ,1},
		{0, -5, 3, 3, 3, 3, -5, 0}};

	int[,] bishopMatrix = {
		{3, 3, -4, 3, 3, -4, 3, 3},
		{3, 4, 4, 4, 4, 4, 4, 3},
		{3, 4, 5, 5, 5, 5, 4, 3},
		{3, 4, 5, 6, 6, 5, 4 ,3},
		{3 ,4 ,5 ,6 ,6 ,5 ,4 ,3},
		{3 ,4 ,5 ,5 ,5 ,5 ,4 ,3},
		{3 ,4 ,4 ,4 ,4 ,4 ,4 ,3},
		{3, 3, -4, 3, 3, -4, 3, 3}};
	int[,] kingMatrix = {
		{3, 14, 12, 0, 0, 1, 15, 3},
		{1, 1, 1, 0, 0, 1, 1, 1}};
	int getKingMatrix(int y, int x)
	{
		if (y>1)
			return -5;
		return kingMatrix[y, x];
	}

	public Move Think(Board board, Timer timer)
	{
		Move[] moves = board.GetLegalMoves();
		return bestMove(board, 3, board.IsWhiteToMove);
	}

	int[] pieceValues = { 0, 100, 300, 300, 500, 900, 99999 };


	float evaluation(Board board)
	{
		evalCounter++;
		float eval = 0;
		for(int i = 0; i < 64; i++)
		{
			Piece piece = board.GetPiece(new Square(i));
			if(piece.IsWhite)
			{
				eval += pieceValues[(int)piece.PieceType];
				switch (piece.PieceType)
				{
					case PieceType.Pawn:
						eval += (piece.Square.Rank - 1);
						break;
					case PieceType.Knight:
						eval += knightMatrix[piece.Square.Rank, piece.Square.File];
						break;
					case PieceType.Bishop:
						eval += bishopMatrix[piece.Square.Rank, piece.Square.File];
						Move[] moves  = board.GetLegalMoves();

						break;
					case PieceType.King:
						if(board.GameMoveHistory.Length < 20)
							eval += getKingMatrix(piece.Square.Rank, piece.Square.File);
						break;
				}
				
			}
			else
			{
				eval -= pieceValues[(int)piece.PieceType];
				switch (piece.PieceType)
				{
					case PieceType.Pawn:
						eval -= (6-piece.Square.Rank);
						break;
					case PieceType.Knight:
						eval -= knightMatrix[piece.Square.Rank, piece.Square.File];
						break;
					case PieceType.Bishop:
						eval -= bishopMatrix[piece.Square.Rank, piece.Square.File];
						break;
					case PieceType.King:
						if (board.GameMoveHistory.Length < 50)
							eval -= getKingMatrix(7 - piece.Square.Rank, piece.Square.File);
						break;
				}
			}
		}
		return eval;
	}


	Move previousBestMove;

	Move bestMove(Board board, int depth, bool playerToMove)
	{
		counter = 0;
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

		Console.WriteLine(counter);
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
			int victimValue = pieceValues[(int)move.CapturePieceType];
			int attackerValue = pieceValues[(int)move.MovePieceType];
			score += 1000 * (victimValue - attackerValue);
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
		counter++;
		if (board.IsRepeatedPosition() || board.IsDraw())
			return 0;
		if (depth == 0)
		{
			return color * evaluation(board);
		}
		if (board.IsInCheckmate())
			return -999999 * color;
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