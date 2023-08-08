using ChessChallenge.API;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq.Expressions;

/// <summary>
/// 
/// Ideas
/// - Kind bot - When winning - go for a draw instead 
/// - No queen bot - just throw it away and try to win anyways
/// - scholars mate / trick bot - always go for certain tricks.
/// </summary>
public class MyBot : IChessBot
{
	int[,] knightMatrix = {
		{1, 2, 3, 3, 3, 3, 2, 1},
		{2, 4, 5, 5, 5, 5, 4, 2},
		{3, 5, 7, 7, 7, 7, 5, 3},
		{3, 5, 7, 8, 8, 7, 5 ,3},
		{3 ,5 ,7 ,8 ,8 ,7 ,5 ,3},
		{3 ,5 ,7 ,7 ,7 ,7 ,5 ,3},
		{2 ,4 ,5 ,5 ,5 ,5 ,4 ,2},
		{1 ,2 ,3 ,3 ,3 ,3 ,2 ,1}};

	int[,] bishopMatrix = {
		{3, 3, 3, 3, 3, 3, 3, 3},
		{3, 4, 4, 4, 4, 4, 4, 3},
		{3, 4, 5, 5, 5, 5, 4, 3},
		{3, 4, 5, 6, 6, 5, 4 ,3},
		{3 ,4 ,5 ,6 ,6 ,5 ,4 ,3},
		{3 ,4 ,5 ,5 ,5 ,5 ,4 ,3},
		{3 ,4 ,4 ,4 ,4 ,4 ,4 ,3},
		{3 ,3 ,3 ,3 ,3 ,3 ,3 ,3}};
	int[,] kingMatrix = {
		{8, 7, 6, 5, 5, 6, 7, 8},
		{7, 6, 5, 4, 4, 5, 6, 7},
		{6, 5, 4, 3, 3 ,4 ,5 ,6},
		{5 ,4 ,3 ,2 ,2 ,3 ,4 ,5},
		{4 ,3 ,2 ,1 ,1 ,2 ,3 ,4},
		{3 ,2 ,1 ,0 ,0 ,1 ,2 ,3},
		{2 ,1 ,0 ,-1 ,-1 ,-0 ,-1 ,-2},
		{1 ,-0 ,-1 ,-2 ,-2 ,-1 ,-0 ,-1}};



	int[] pieceValues = { 0, 100, 300, 300, 500, 900, 99999 };

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
		for(int i = 0; i < 64; i++)
		{
			Piece piece = board.GetPiece(new Square(i));
			if(piece.IsWhite)
			{
				eval += pieceValues[(int)piece.PieceType];
				switch (piece.PieceType)
				{
					case PieceType.Pawn:
						eval += (piece.Square.Rank - 1) * (piece.Square.Rank - 1);
						break;
					case PieceType.Knight:
						eval += knightMatrix[piece.Square.File, piece.Square.Rank];
						break;
					case PieceType.Bishop:
						eval += bishopMatrix[piece.Square.File, piece.Square.Rank];
						break;
					case PieceType.King:
						if(board.GameMoveHistory.Length < 20)
							eval -= kingMatrix[piece.Square.File, piece.Square.Rank];
						break;
				}
				
			}
			else
			{
				eval -= pieceValues[(int)piece.PieceType];
				switch (piece.PieceType)
				{
					case PieceType.Pawn:
						eval -= (6-piece.Square.Rank ) * (6-piece.Square.Rank);
						break;
					case PieceType.Knight:
						eval -= knightMatrix[piece.Square.File, piece.Square.Rank];
						break;
					case PieceType.Bishop:
						eval -= bishopMatrix[piece.Square.File, piece.Square.Rank];
						break;
					case PieceType.King:
						if (board.GameMoveHistory.Length < 20)
							eval -= kingMatrix[piece.Square.File, 8 - piece.Square.Rank];
						break;
				}
			}
		}
		return eval;
	}
	Move bestMove(Board board, int depth, bool playerToMove)
	{
		Move bestmove = board.GetLegalMoves()[0];
		float curreval = evaluation(board);
		int sign = playerToMove ? -1 : 1;
		float bestEval = 1000000 * sign;
		if (playerToMove) //if white wants the best move
		{
			foreach (Move move in board.GetLegalMoves())
			{
				board.MakeMove(move);
				float eval = minmax(board, depth, -1000000, 1000000, !playerToMove); //is opposite turn after having done a move
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
				float eval = minmax(board, depth, -1000000, 1000000, !playerToMove); //is opposite turn after having done a move
				if (eval < bestEval)
				{
					bestEval = eval;
					bestmove = move;
				}
				board.UndoMove(move);
			}
		}
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
		if (board.IsRepeatedPosition() || board.IsDraw())
			return 0;
		if (depth == 0)
		{
			return evaluation(board);
		}
		if(isMaximizingPlayer)
		{
			if (board.IsInCheckmate())
				return -999999;
			float max = -100000;
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
				return 999999;
			float min = 100000;
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