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

	List<int> counters = new List<int>();
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
		// Initialize variables for pawn structure and king safety evaluation
		ulong whitePawns = board.GetPieceBitboard(PieceType.Pawn, true);
		ulong blackPawns = board.GetPieceBitboard(PieceType.Pawn, false);
		ulong whiteIsolatedPawns = ~((whitePawns << 1) | (whitePawns >> 1));
		ulong blackIsolatedPawns = ~((blackPawns << 1) | (blackPawns >> 1));
		ulong whitePieces = board.WhitePiecesBitboard;
		ulong blackPieces = board.BlackPiecesBitboard;
		Square whiteKingSquare = board.GetKingSquare(true);
		Square blackKingSquare = board.GetKingSquare(false);
		eval += materialEval(board, true);
		eval -= materialEval(board, false);
		// Evaluate doubled pawns
		for (int file = 0; file < 8; file++)
		{
			int whitePawnsOnFile = BitOperations.PopCount(whitePawns & FileMask(file));
			int blackPawnsOnFile = BitOperations.PopCount(blackPawns & FileMask(file));
			if (whitePawnsOnFile > 1)
				eval -= whitePawnsOnFile - 1;
			if (blackPawnsOnFile > 1)
				eval += blackPawnsOnFile - 1;
		}

		// Evaluate isolated pawns
		eval -= BitOperations.PopCount(whiteIsolatedPawns & whitePawns);
		eval += BitOperations.PopCount(blackIsolatedPawns & blackPawns);

		// Evaluate king safety
		int whiteKingFile = whiteKingSquare.File;
		int whiteKingRank = whiteKingSquare.Rank;

		// Check for open files towards the king
		ulong fileMask = FileMask(whiteKingFile);
		if ((fileMask & ~(RankMask(whiteKingRank) - 1) & ~(whitePieces | blackPieces)) == (fileMask & ~(RankMask(whiteKingRank) - 1)))
			eval -= 0.5f;

		// Check for open diagonals towards the king
		ulong diagonalMask = DiagonalMask(whiteKingSquare);
		if ((diagonalMask & ~(RankMask(whiteKingRank) - 1) & ~(whitePieces | blackPieces)) == (diagonalMask & ~(RankMask(whiteKingRank) - 1)))
			eval -= 0.5f;

		// Evaluate king safety for black
		int blackKingFile = blackKingSquare.File;
		int blackKingRank = blackKingSquare.Rank;

		// Check for open files towards the king
		fileMask = FileMask(blackKingFile);
		if ((fileMask & (RankMask(blackKingRank + 1) - 1) & ~(whitePieces | blackPieces)) == (fileMask & (RankMask(blackKingRank + 1) - 1)))
			eval += 0.5f;

		// Check for open diagonals towards the king
		diagonalMask = DiagonalMask(blackKingSquare);
		if ((diagonalMask & (RankMask(blackKingRank + 1) - 1) & ~(whitePieces | blackPieces)) == (diagonalMask & (RankMask(blackKingRank + 1) - 1)))
			eval += 0.5f;

		return eval;
	}
	ulong FileMask(int file)
	{
		return 0x0101010101010101UL << file;
	}
	ulong RankMask(int rank)
	{
		return 0xffUL << (rank * 8);
	}
	ulong DiagonalMask(Square square)
	{
		int rank = square.Rank;
		int file = square.File;
		ulong mask = 0;
		for (int i = 1; i < Math.Min(rank, file) + 1; i++)
			mask |= 1UL << ((rank - i) * 8 + file - i);
		for (int i = 1; i < Math.Min(7 - rank, file) + 1; i++)
			mask |= 1UL << ((rank + i) * 8 + file - i);
		return mask;
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
			score += 10*pieceValues[(int)move.CapturePieceType-1] - pieceValues[(int)move.MovePieceType-1];
			if (!board.SquareIsAttackedByOpponent(move.TargetSquare))
				score = 100000;
		}
		if (move.IsPromotion)
			score += 900;
		//give small bonus for perhaps retaking or taking the last piece moved.
		if (board.GameMoveHistory.Length > 0 && board.GameMoveHistory[^1].TargetSquare == move.TargetSquare)
			score += 50;
		// Give a small bonus for checks
		board.MakeMove(move);
		if (board.IsInCheck())
			score += 100;
		board.UndoMove(move);
		if (board.SquareIsAttackedByOpponent(move.TargetSquare))
			score -= pieceValues[(int)move.MovePieceType - 1];

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