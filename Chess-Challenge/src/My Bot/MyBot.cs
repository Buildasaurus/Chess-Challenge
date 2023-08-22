using ChessChallenge.API;
using System.Collections.Generic;
using System;
using System.Linq;
using static System.Formats.Asn1.AsnWriter;

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
	
	int[,] mg_knight_table = {
		{-167, -89, -34, -49,  61, -97, -15, -107},
		{-73, -41,  72,  36,  23,  62,   7,  -17},
		{-47,  60,  37,  65,  84, 129,  73,   44},
		{-9,  17,  19,  53,  37,  69,  18,   22},
		{-13,   4,  16,  13,  28,  19,  21,   -8},
		{-23, -9 ,12 ,10 ,19 ,17 ,25 ,-16},
		{-29 ,-53 ,-12 ,-3 ,-1 ,18 ,-14 ,-19},
		{-105 ,-21 ,-58 ,-33 ,-17 ,-28 ,-19 ,-23}
	};

	int[,] mg_bishop_table = {
		{-29   ,4   ,-82   ,-37   ,-25   ,-42   ,7   ,-8 },
		{-26   ,16   ,-18   ,-13   ,30   ,59   ,18   ,-47 },
		{-16   ,37   ,43   ,40   ,35   ,50   ,37   ,-2 },
		{-4    ,5    ,19    ,50    ,37    ,37    ,7    ,-2 },
		{-6    ,13    ,13    ,26    ,34    ,12    ,10    ,4 },
		{0     ,15     ,15     ,15     ,14     ,27     ,18     ,10 },
		{4     ,15     ,16      ,0      ,7      ,21      ,33      ,1 },
		{-33    ,-3     ,-14     ,-21     ,-13     ,-12     ,-39     ,-21 }
	};


	int[,] mg_rook_table = {
		{ 32,  42,  32,  51, 63,  9,  31,  43},
		{ 27,  32,  58,  62, 80, 67,  26,  44},
		{ -5,  19,  26,  36, 17, 45,  61,  16},
		{-24, -11,   7,  26, 24, 35,  -8, -20},
		{-36, -26, -12,  -1,  9, -7,   6, -23},
		{-45, -25, -16, -17,  3,  0,  -5, -33},
		{-44, -16, -20,  -9, -1, 11,  -6, -71},
		{-19, -13,   1,  17, 16,  7, -37, -26}
	};

	int[,] mg_queen_table = {
		{-28,   0,  29,  12,  59,  44,  43,  45},
		{-24, -39,  -5,   1, -16,  57,  28,  54},
		{-13, -17,   7,   8,  29,  56,  47,  57},
		{-27, -27, -16, -16,  -1,  17,  -2,   1},
		{ -9, -26,  -9, -10,  -2,  -4,   3,  -3},
		{-14,   2, -11,  -2,  -5,   2, 14 ,   5},
		{-35 , -8 ,11 ,   2 ,   8 ,15 ,-3 ,   1},
		{ -1 ,-18 ,-9 ,10 ,-15 ,-25 ,-31 ,-50}
	};
	int[,] mg_pawn_table = {
		{0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 },
		{98 ,134 ,61 ,95 ,68 ,126 ,34 ,-11},
		{-6 ,7 ,26 ,31 ,65 ,56 ,25 ,-20},
		{-14 ,13 ,6 ,21 ,23 ,12 ,17 ,-23},
		{-27 ,-2 ,-5 ,12 ,17 ,6 ,10 ,-25},
		{-26 ,-4 ,-4 ,-10 ,3 ,3 ,33 ,-12},
		{-35 ,-1 ,-20 ,-23 ,-15 ,24 ,38 ,-22},
		{0 ,0 ,0 ,0 ,0 ,0 ,0 ,0}
	};
	int[,] eg_rook_table = {
		{13, 10, 18, 15, 12, 12, 8, 5},
		{11, 13, 13, 11, -3, 3, 8, 3},
		{7, 7, 7, 5, 4, -3, -5, -3},
		{4, 3, 13, 1, 2, 1, -1, 2},
		{3, 5, 8, 4, -5, -6, -8,-11},
		{-4,0,-5,-1,-7,-12,-8,-16},
		{-6,-6,0 ,2 ,-9 ,-9 ,-11,-3 },
		{-9 ,2 ,3 ,-1 ,-5 ,-13 ,4 ,-20}
	};
	int[,] mg_king_table = {
		{-65,23,16,-15,-56,-34,2,13},
		{29,-1,-20,-7,-8,-4,-38,-29},
		{-9,24,2,-16,-20,6,22,-22},
		{-17,-20,-12,-27,-30,-25,-14,-36},
		{-49,-1,-27,-39,-46,-44,-33,-51},
		{-14,-14,-22,-46,-44,-30,-15,-27},
		{1,7,-8,-64,-43,-16,9,8},
		{-15,36,12,-54,8,-28,24,14}
	};
	int[] phase_weight = { 0, 1, 1, 2, 4, 0 };

	int[,] eg_pawn_table = {
		{0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 },
		{178 ,173 ,158 ,134 ,147 ,132 ,165 ,187 },
		{94 ,100 ,85 ,67 ,56 ,53 ,82 ,84 },
		{32 ,24 ,13 ,5 ,-2 ,4 ,17 ,17 },
		{13 ,9 ,-3 ,-7 ,-7 ,-8 ,3 ,-1 },
		{4 ,7 ,-6 ,1 ,0 ,-5 ,-1 ,-8 },
		{13 ,8 ,8 ,10 ,13 ,0 ,2 ,-7 },
		{0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 }
	};




	bool playerColor;
	bool timeToStop = false;
	ChessChallenge.API.Timer timer;
	/// <summary>
	/// if under this miliseconds remaing, then should stop.
	/// </summary>
	int endMiliseconds = 0;
	public Move Think(Board board, ChessChallenge.API.Timer _timer)
	{
		Console.WriteLine("-----My bot thinking----");
		//killerMoves.Clear();
		timer = _timer;
		historyTable = new int[2, 7, 64];
		playerColor = board.IsWhiteToMove;
		endMiliseconds = Math.Min(timer.MillisecondsRemaining - 50, timer.MillisecondsRemaining*29/30);
		timeToStop = false;
		return bestMove(board, board.IsWhiteToMove);
	}
	int[] pieceValues = { 82, 337, 365, 477, 1025, 0 };
	int[] endGameValues = { 94, 281, 297, 512, 936, 0 };

	Move overAllBestMove;
	Move bestMove(Board board, bool playerToMove)
	{
		lookups = 0;
		entryCount = 0;
		counters.Add(0);
		int color = playerToMove ? 1 : -1;
		int bestEval = 0;
		int thinkStart = timer.MillisecondsRemaining;
		for (sbyte d = 1; d <= 32; d++)
		{
			if (timeToStop) break;
			startime = timer.MillisecondsRemaining;

			bestEval = -negamax(board, d, 0, -10000000, 10000000, color);
			Console.WriteLine($"info string best move at depth {d} was {overAllBestMove} with eval at {bestEval}");
			Console.WriteLine($"info string Time used for depth {d}: {startime - timer.MillisecondsRemaining} miliseconds");

		}
		Console.WriteLine("info string -------node count------- " + counters[^1]);
		Console.WriteLine("info string useful lookups:  " + lookups);
		Console.WriteLine("info string Entry count " + entryCount);
		Console.WriteLine($"info string Final best move was {overAllBestMove} with eval at {bestEval}");
		Console.WriteLine($"info string Time used for completed search: {thinkStart - timer.MillisecondsRemaining} miliseconds");

		if (overAllBestMove == Move.NullMove) overAllBestMove = board.GetLegalMoves()[0]; // just in case there basically is no time.

		return overAllBestMove;
	}



	int MoveOrderingHeuristic(Move move, Board board, Move goodMove)
	{
		int score = 0;
		if (move == goodMove) score = 100000000;
		// Give higher scores to captures and promotions
		if (move.IsCapture)
		{
			// Use MVV-LVA heuristic
			score += 10 * pieceValues[(int)move.CapturePieceType - 1] - pieceValues[(int)move.MovePieceType - 1];
		}
		if (move.IsPromotion)
			score += 900;
		//If this move has caused lots of cutoffs, let's put it higher.
		score += historyTable[board.IsWhiteToMove ? 0 : 1, (int)move.MovePieceType, move.TargetSquare.Index];
		return score;
	}
	int evaluation(Board board)
	{
		int eval = 0;
		int gamePhase = 24;
		for (int i = 0; i < 5; i++)
		{
			int piececount = board.GetPieceList((PieceType)(i + 1), false).Count + board.GetPieceList((PieceType)(i + 1), true).Count;
			gamePhase -= piececount * phase_weight[i];
		}
		gamePhase = Math.Max(gamePhase, 0);
		foreach (PieceList pieces in board.GetAllPieceLists())
		{
			int playerSign = pieces.IsWhitePieceList ? 1 : -1;
			int openingEval = pieces.Count * pieceValues[(int)pieces.TypeOfPieceInList - 1];
			int endgameEval = pieces.Count * endGameValues[(int)pieces.TypeOfPieceInList - 1];
			foreach (Piece piece in pieces)
			{
				int rank = piece.IsWhite ? 7 - piece.Square.Rank : piece.Square.Rank;
				int file = piece.Square.File;

				switch (piece.PieceType)
				{
					case PieceType.Pawn:
						openingEval += mg_pawn_table[rank, file];
						endgameEval += eg_pawn_table[rank, file];
						break;
					case PieceType.Knight:
						openingEval += mg_knight_table[rank, file];
						endgameEval += mg_knight_table[rank, file];
						break;
					case PieceType.Bishop:
						openingEval += mg_bishop_table[rank, file];
						endgameEval += mg_bishop_table[rank, file];
						break;
					case PieceType.Queen:
						openingEval += mg_queen_table[rank, file];
						endgameEval += mg_queen_table[rank, file];
						break;
					case PieceType.Rook:
						openingEval += mg_rook_table[rank, file];
						endgameEval += eg_rook_table[rank, file]; 
						break;
					case PieceType.King:
						openingEval += mg_king_table[rank, file];
						endgameEval	+= mg_knight_table[rank, file];
						break;
				}
			}
			eval += ((openingEval * (24 - gamePhase)) + (endgameEval * gamePhase)) / 24 * playerSign;

		}
		return eval;
	}


	// History table definition
	int[,,] historyTable;
	// Define a structure to store transposition table entries
	struct Transposition
	{
		public ulong zobristHash;
		public Move move;
		public int evaluation;
		public sbyte depth;
		public byte flag;
	};
	int lookups = 0;
	int entryCount = 0;
	int startime;


	// Create a transposition table
	Transposition[] transpositionTable = new Transposition[0x800000];
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
	int negamax(Board board, sbyte depth, int ply, int alpha, int beta, int color)
	{
		//Much used variables
		bool isPV = beta - alpha > 1;
		bool notRoot = ply > 0;
		bool isInCheck = board.IsInCheck();

		//Draw detection
		if (notRoot && board.IsDraw())
			return 0;
		if (board.IsInCheckmate())
			return ply - 999999;

		//Debug
		counters[^1]++;



		//check extensions - MUST BE BEFORE QSEARCH	
		if (isInCheck)
			depth = (depth < 0) ? (sbyte)1 : (sbyte)(depth + 1);
		
		//QSearch
		bool isQSearch = depth <= 0;
		if (isQSearch)
		{
			int standingPat = isInCheck ? -998999 : color * evaluation(board);

			if (standingPat >= beta)
			{
				return beta;
			}

			if (alpha < standingPat)
			{
				alpha = standingPat;
			}
		}

		// Null move pruning - is perhaps best with more time on the clock?
		const int R = 2;
		if (!isPV && !isInCheck && depth > R)
		{
			board.TrySkipTurn();
			int score = -negamax(board, (sbyte)(depth - R - 1), ply + 1, -beta, -alpha, -color);
			board.UndoSkipTurn();
			if (score >= beta)
				return beta;
		}
		// Transposition table lookup
		ulong zobristHash = board.ZobristKey;
		ref Transposition transposition = ref transpositionTable[zobristHash & 0x7FFFFF];
		Move bestMove = Move.NullMove;
		if (transposition.zobristHash == zobristHash && transposition.depth >= depth)
		{
			lookups++;
			//If we have an "exact" score (a < score < beta) just use that
			//If we have a lower bound better than beta, use that
			//If we have an upper bound worse than alpha, use that
			if ((transposition.flag == 1) || 
				(transposition.flag == 2 && transposition.evaluation >= beta) || 
				(transposition.flag == 3 && transposition.evaluation <= alpha)) return transposition.evaluation;
			bestMove = transposition.move;
		}

		if (!notRoot) Console.WriteLine($"info string Bestmove at depth{depth} was for a starter: {overAllBestMove}");

		// Generate legal moves and sort them
		Move goodMove = notRoot ? bestMove : overAllBestMove;

		// Gamestate, checkmate and draws - THIS PLACEMENT HASN'T BEEN TESTED - EARLIER IT WAS BEFORE QSEARCH - THIS PLACEMENT IS RUNNING TEST
		Move[] legalmoves = board.GetLegalMoves(isQSearch);
		Array.Sort(legalmoves, (a, b) => MoveOrderingHeuristic(b, board, goodMove).CompareTo(MoveOrderingHeuristic(a, board, goodMove)));
		// if we are at root level, make sure that the overallbest move from earlier iterations is at top.
		Move bestFoundMove = Move.NullMove;

		//start searching
		int max = -100000000;
		int moveCount = 0; // Add a move counter
		foreach (Move move in legalmoves)
		{
			moveCount++; // Increment the move counter

			//Early stop at top level
			if (!timeToStop && timer.MillisecondsRemaining < endMiliseconds) timeToStop = true;
			if (timeToStop)
			{
				return 0;
			}

			board.MakeMove(move);

			if(isQSearch)
			{
				int score = -negamax(board, 0, ply + 1, -beta, -alpha, -color);
				board.UndoMove(move);

				if (score >= beta)
				{
					return beta;
				}
				if (score > alpha)
				{
					alpha = score;
				}
			}
			else
			{
				// LMR: reduce the depth of the search for moves beyond a certain move count threshold
				int reduction = (int)((depth >= 4 && moveCount >= 4 && !board.IsInCheck() && !move.IsCapture && !move.IsPromotion && !isInCheck && !isPV) ? 1 + Math.Log2(depth) * Math.Log2(moveCount) / 2 : 0);
				//reduction = isPV && reduction > 0 ? 1 : 0;

				int eval;
				if (moveCount == 1)
					eval = -negamax(board, (sbyte)(depth - 1 - reduction), ply + 1, -beta, -alpha, -color);
				else
				{
					eval = -negamax(board, (sbyte)(depth - 1 - reduction), ply + 1, -alpha - 1, -alpha, -color);
					if (eval > alpha || (reduction > 0 && beta > eval))
						eval = -negamax(board, (sbyte)(depth - 1), ply + 1, -beta, -alpha, -color);
				}

				if (eval > max)
				{
					//if root level new best move is found, then save it to be played or for next iteration
					if (ply == 0 && !timeToStop)
					{
						overAllBestMove = move;
						Console.WriteLine($"info string new Overall Best move: {move}");
					}
					bestFoundMove = move;
					max = eval;
				}
				board.UndoMove(move);
				alpha = Math.Max(alpha, max);
				if (alpha >= beta)
				{
					//if move causes beta-cutoff, it's nice, so it's "score" is now increased, depending on how early it did the beta-cutoff. yes?
					historyTable[board.IsWhiteToMove ? 0 : 1, (int)move.MovePieceType, move.TargetSquare.Index] += depth * depth;
					break;
				}
			}
		}

		if (isQSearch) return alpha;
		storeEntry(ref transposition, depth, alpha, beta, max, bestFoundMove, zobristHash);
		return max;
	}



	void storeEntry(ref Transposition transposition, sbyte depth, int alpha, int beta, int bestEvaluation, Move bestMove, ulong zobristHash)
	{
		entryCount++;

		// Transposition table store
		transposition.evaluation = bestEvaluation;
		transposition.zobristHash = zobristHash;
		transposition.move = bestMove;
		if (bestEvaluation > alpha)
			transposition.flag = 1; //"exact" score
		else if (bestEvaluation >= beta)
		{
			transposition.flag = 2; //lower bound
		}
		else
			transposition.flag = 3; //upper bound
		transposition.depth = (sbyte)depth;
	}
}