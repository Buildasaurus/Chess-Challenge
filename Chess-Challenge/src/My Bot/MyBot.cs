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
    bool timeToStop = false;
    Timer timer;
    /// <summary>
    /// if under this miliseconds remaing, then should stop.
    /// </summary>
    int endMiliseconds = 0;
    public Move Think(Board board, Timer _timer)
    {
		Console.WriteLine("-----My bot thinking----");
		//killerMoves.Clear();
		timer = _timer;
        endMiliseconds = (int)Math.Ceiling(timer.MillisecondsRemaining*0.985f);
        timeToStop = false;
        return bestMove(board, board.IsWhiteToMove);
    }
    int[] pieceValues = { 100, 300, 300, 500, 900, 99999 };
	Move overAllBestMove;
    Move bestMove(Board board, bool playerToMove)
    {
		lookups = 0;
		entryCount = 0;
        counters.Add(0);
		int color = playerToMove ? 1 : -1;
		int bestEval = 0;
		int thinkStart = timer.MillisecondsRemaining;
		for (int d = 1; d <= 32; d++)
        {
			if (timeToStop) break;
			startime = timer.MillisecondsRemaining;

			bestEval = -negamax(board, d, 0, -10000000, 10000000, color, 0);
			Console.WriteLine($"bestmove at depth {d} was {overAllBestMove} with eval at {bestEval}");
			Console.WriteLine($"Time used for depth {d}: {startime - timer.MillisecondsRemaining} miliseconds");

		}
		Console.WriteLine("-------node count------- " + counters[^1]);
		Console.WriteLine("useful lookups:  " + lookups);
		Console.WriteLine("lookuptable count " + transpositionTable.Count);
		Console.WriteLine("Entry count " + entryCount);
		Console.WriteLine($"Final best move was {overAllBestMove} with eval at {bestEval}");
		Console.WriteLine($"Time used for completed search: {thinkStart - timer.MillisecondsRemaining} miliseconds");



		return overAllBestMove;
    }
	


	int MoveOrderingHeuristic(Move move, Board board)
	{
		int score = 0;

		// Give higher scores to captures and promotions
		if (move.IsCapture)
		{
			// Use MVV-LVA heuristic
			score += 10 * pieceValues[(int)move.CapturePieceType - 1] - pieceValues[(int)move.MovePieceType - 1];
		}
		if (move.IsPromotion)
			score += 900;
		return score;
	}
	int evaluation(Board board)
    {
        int eval = 0;
        eval += materialEval(board, true);
        eval -= materialEval(board, false);

        // Initialize variables for pawn structure and king safety evaluation
        /*ulong whitePawns = board.GetPieceBitboard(PieceType.Pawn, true);
		ulong blackPawns = board.GetPieceBitboard(PieceType.Pawn, false);
		ulong whiteIsolatedPawns = ~((whitePawns << 1) | (whitePawns >> 1));
		ulong blackIsolatedPawns = ~((blackPawns << 1) | (blackPawns >> 1));
		ulong whitePieces = board.WhitePiecesBitboard;
		ulong blackPieces = board.BlackPiecesBitboard;
		Square whiteKingSquare = board.GetKingSquare(true);
		Square blackKingSquare = board.GetKingSquare(false);

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
		*/
        return eval;
    }
    /*
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
	}*/

    int materialEval(Board board, bool color)
    {
        int eval = 0;
        for (int i = 0; i < 6; i++)
        {
            PieceList pieces = board.GetPieceList((PieceType)(i+1), color);
            eval += pieceValues[i] * pieces.Count;
            foreach (Piece piece in pieces)
            {
                switch (piece.PieceType)
                {
                    case PieceType.Pawn:
                        eval += 10 * (piece.IsWhite ? piece.Square.Rank - 1 : 6 - piece.Square.Rank);
                        break;
                    case PieceType.Knight:
                        eval += knightMatrix[piece.Square.Rank, piece.Square.File];
                        break;
                    case PieceType.Bishop:
                        eval += bishopMatrix[piece.Square.Rank, piece.Square.File];
                        break;
                    case PieceType.King:
                        if (board.GameMoveHistory.Length < 40)
                            eval += getKingMatrix(piece.IsWhite ? piece.Square.Rank : 7 - piece.Square.Rank, piece.Square.File);
                        break;
                }
            }
        }
        return eval;
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
	void MoveToFrontOfArray(ref Move[] array, Move move)
	{
		int index = Array.IndexOf(array, move);
		if (index > 0)
		{
			for (int i = index; i > 0; i--)
			{
				array[i] = array[i - 1];
			}
			array[0] = move;
		}
	}
	
	// Define a structure to store transposition table entries
	struct TTEntry
	{
		public int value;
		public int depth;
		public int type; // 0 = exact, 1 = lower bound, 2 = upper bound
		public Move bestMove;
	}
	int lookups = 0;
	int entryCount = 0;
	int startime;


	// Create a transposition table
	Dictionary<ulong, TTEntry> transpositionTable = new Dictionary<ulong, TTEntry>(5000000);
	// Define a data structure to store killer moves
	//Dictionary<Move, int> killerMoves = new Dictionary<Move, int>();
	/// <summary>
	/// 
	/// </summary>
	/// <param name="board"></param>
	/// <param name="depth">Depth left to search - falls</param>
	/// <param name="ply">How deep you have made it - rises</param>
	/// <param name="alpha"></param>
	/// <param name="beta"></param>
	/// <param name="color"></param>
	/// <param name="numExtensions"></param>
	/// <returns></returns>
	int negamax(Board board, int depth, int ply, int alpha, int beta, int color, int numExtensions)
	{
		bool notRoot = ply > 0;

		// Transposition table lookup
		Move bestMove = Move.NullMove;
		ulong zobristHash = board.ZobristKey;
		if (transpositionTable.ContainsKey(zobristHash))
		{
			TTEntry entry = transpositionTable[zobristHash];
			if (entry.depth >= depth)
			{
				lookups++;
				bestMove = entry.bestMove;
				//if (entry.type == 0) // exact value
				//	return entry.value;
				if (entry.type == 1) // lower bound
					alpha = Math.Max(alpha, entry.value);
				else // upper bound
					beta = Math.Min(beta, entry.value);

				//if (alpha >= beta)
				//	return entry.value;
			}
		}

		if (!notRoot) Console.WriteLine($"Bestmove at depth{depth} was for a starter: {overAllBestMove}");

		//return early statements.
		if (board.IsInCheckmate())
			return -999999;
		counters[^1]++;
		if (notRoot && (board.IsRepeatedPosition() || board.IsDraw() || board.IsInStalemate()))
			return 0;
		if (depth == 0)
		{
			return color * evaluation(board);
		}

		// Generate legal moves and sort them
		Move[] legalmoves = board.GetLegalMoves();
		Array.Sort(legalmoves, (a, b) => MoveOrderingHeuristic(b, board).CompareTo(MoveOrderingHeuristic(a, board)));
		// if we are at root level, make sure that the overallbest move from earlier iterations is at top.
		if (!notRoot && overAllBestMove != Move.NullMove)
		{
			MoveToFrontOfArray(ref legalmoves, overAllBestMove);
		}
		//move the best move from lookup to top.
		else if (notRoot && bestMove != Move.NullMove)
		{
			MoveToFrontOfArray(ref legalmoves, bestMove);
		}
		Move bestFoundMove = Move.NullMove;

		//start searching
		int max = -100000000;
		foreach (Move move in legalmoves)
		{
			//Early stop at top level
			if (!timeToStop && timer.MillisecondsRemaining < endMiliseconds) timeToStop = true;
			if (timeToStop && !notRoot)
			{
				break;
			}

			board.MakeMove(move);
			int extension = (board.IsInCheck() || (board.GameMoveHistory.Length > 0) && (move.TargetSquare == board.GameMoveHistory[^1].TargetSquare)) && numExtensions < 16 ? 1 : 0;

			int eval = -negamax(board, depth - 1 + extension, ply + 1, -beta, -alpha, -color, numExtensions);

			if (eval > max)
			{
				//if root level new best move is found, then save it to be played or for next iteration
				if(ply == 0)
				{
					overAllBestMove = move;
					Console.WriteLine($"new Overall Best move: {move}");
				}
				bestFoundMove = move;
				max = eval;
			}
			board.UndoMove(move);
			alpha = Math.Max(alpha, max);
			if (alpha >= beta && notRoot) //alpha > beta shouldn't be possible at root, but anyways.
			{
				storeEntry(depth, alpha, beta, max, bestFoundMove, zobristHash);

				return max;
			}

		}

		storeEntry(depth, alpha, beta, max, overAllBestMove, zobristHash);
		return max;
	}
	void storeEntry(int depth, int alpha, int beta, int max, Move bestmove, ulong zobristHash)
	{
		entryCount++;

		// Transposition table store
		TTEntry newEntry;
		newEntry.depth = depth;
		newEntry.value = max;
		if (alpha > max)
			newEntry.type = 2; // upper bound
		else if (beta < max)
			newEntry.type = 1; // lower bound
		else
			newEntry.type = 0; // exact value
		newEntry.bestMove = bestmove;

		transpositionTable[zobristHash] = newEntry;
	}
}