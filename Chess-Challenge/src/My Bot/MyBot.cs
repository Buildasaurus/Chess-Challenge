using ChessChallenge.API;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Transactions;
using System.Security;

/// <summary>
/// 
/// Ideas
/// - Kind bot - When winning - go for a draw instead 
/// - No queen bot - just throw it away and try to win anyways
/// - scholars mate / trick bot - always go for certain tricks.
/// </summary>
public class MyBot : IChessBot
{
	int[] pieceValues = { 0, 1, 3, 3, 5, 9, 999 };

	public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        return bestMove(board, 3);
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
	int evaluation(Board board)
	{
		int eval = 0;
		for(int i = 0; i < 64; i++)
		{
			Piece piece = board.GetPiece(new Square(i));
			if(piece.IsWhite)
			{
				eval += pieceValues[(int)piece.PieceType];
			}
			else
			{
				eval -= pieceValues[(int)piece.PieceType];
			}
		}
		return eval;
	}
	Move bestMove(Board board, int depth)
	{
		Move bestmove = board.GetLegalMoves()[0];
		int curreval = evaluation(board);
		//assuming pc is black
		int bestEval = 100;
		if (board.IsWhiteToMove)
		{
			foreach (Move move in board.GetLegalMoves())
			{
				board.MakeMove(move);
				int eval = min(board, depth); //is black turn, who will want to maximize
				if (eval > bestEval)
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
				int eval = max(board, depth);
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
	int min(Board board, int depth)
	{
		if (depth == 0)
		{
			return evaluation(board);
		}
		int min = 1000;
		foreach (Move move in board.GetLegalMoves())
		{
			board.MakeMove(move);
			int eval = max(board, depth - 1);
			if (eval < min)
			{
				min = eval;
			}
			board.UndoMove(move);
		}
		return min;
	}
	int max(Board board, int depth)
	{
		if (depth == 0)
		{
			return evaluation(board);
		}
		int max = -1000;
		foreach (Move move in board.GetLegalMoves())
		{
			board.MakeMove(move);
			int eval = min(board, depth - 1);
			if (eval > max)
			{
				max = eval;
			}
			board.UndoMove(move);

		}
		return max;
	}
}