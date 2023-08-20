using ChessChallenge.API;
using System.Collections.Generic;
using System;
using System.Linq;

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
		{-10, 0, 10, 10, 10, 10, 0,-10},
		{-10,  0, 10, 10, 10, 10,  0,-10},
		{-10, 5, 5, 10, 10, 5, 5,-10},
		{-10, 10, 5, 10, 10, 5, 10,-10},
		{-10,5 ,0 ,0 ,0 ,0 ,5 ,-10},
		{-20,-10,-40,-30,-20,-30,-40,-20},
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
	int[,] pawnMatrix = {
		{0, 0, 0, 0, 0, 0, 0, 0},
		{5, 10, 10, -20, -20, 10, 10, 5},
		{5, -5, -10, 0, 0, -10, -5, 5},
		{0, 0, 0, 20, 20, 0, 0, 0},
		{5, 5, 10, 25, 25, 10, 5, 5},
		{10, 10, 20, 30, 30, 20, 10, 10},
		{50,50 ,50 ,50 ,50 ,50 ,50 ,50},
		{0 ,0 ,0 ,0 ,0 ,0 ,0 ,0}
	};
	int[,] Rooks =  {
		{0,  0,  0,  5,  5,  0,  0,  0},
		{-5,  0,  0,  0,  0,  0,  0, -5},
		{-5,  0,  0,  0,  0,  0,  0, -5},
		{-5,  0,  0,  0,  0,  0,  0, -5},
		{-5,  0,  0,  0,  0,    0 ,     0 , -5},
		{-5 ,   0 ,     0 ,     0 ,     0 ,     0 ,     0 , -5},
		{5 ,10 ,10 ,10 ,10 ,10 ,10 ,5},
		{0 ,    0 ,     0 ,     0 ,     0 ,     0 ,     0 ,     0}
	};

	int[,] Queens = {
		{-20,-10,-10,-5,-5,-10,-10,-20},
		{-10,  0,  5,  0,  0,  0,  0,-10},
		{-10,  5,  5,  5,  5,  5,  0,-10},
		{0,  0,  5,  5,  5,  5,  0, -5,},
		{-5,  0,  5,  5,  5,  5,  0, -5,},
		{-10,  0,  5,  5,  5,  5,  0,-10,},
		{-10,  0,  0,  0,  0,  0,  0,-10,},
		{-20,-10,-10,-5,-5,-10,-10,-20}
	};

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

		endMiliseconds = (int)Math.Ceiling(timer.MillisecondsRemaining * 0.985f);
		timeToStop = false;
		return bestMove(board, board.IsWhiteToMove);
	}
	int[] pieceValues = { 100, 300, 320, 500, 900, 99999 };
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
		return score;
	}
	int evaluation(Board board)
	{
		int eval = 0;
		eval += materialEval(board, true);
		eval -= materialEval(board, false);

		return eval;
	}


	int materialEval(Board board, bool color)
	{
		int eval = 0;
		int gamePhase = 3900;
		for (int i = 0; i < 5; i++)
		{
			PieceList pieces = board.GetPieceList((PieceType)(i + 1), color);
			eval += pieceValues[i] * pieces.Count;
		}
		gamePhase -= eval;
		for (int i = 0; i < 6; i++)
		{
			PieceList pieces = board.GetPieceList((PieceType)(i + 1), color);
			foreach (Piece piece in pieces)
			{
				switch (piece.PieceType)
				{
					case PieceType.Pawn:
						int openingEval = pawnMatrix[piece.IsWhite ? piece.Square.Rank : 7 - piece.Square.Rank, piece.Square.File];
						int endgameEval = 10 * (piece.IsWhite ? piece.Square.Rank - 1 : 6 - piece.Square.Rank);
						eval += ((openingEval * (3900 - gamePhase)) + (endgameEval * gamePhase)) / 3900;
						break;
					case PieceType.Knight:
						eval += knightMatrix[piece.Square.Rank, piece.Square.File];
						break;
					case PieceType.Bishop:
						eval += bishopMatrix[piece.Square.Rank, piece.Square.File];
						break;
					case PieceType.Queen:
						eval += Queens[piece.IsWhite ? piece.Square.Rank : 7 - piece.Square.Rank, piece.Square.File];
						break;
					case PieceType.Rook:
						eval += Rooks[piece.IsWhite ? piece.Square.Rank : 7 - piece.Square.Rank, piece.Square.File];
						break;
					case PieceType.King:
						openingEval = getKingMatrix(piece.IsWhite ? piece.Square.Rank : 7 - piece.Square.Rank, piece.Square.File);
						endgameEval = knightMatrix[piece.Square.Rank, piece.Square.File];
						eval += ((openingEval * (3900 - gamePhase)) + (endgameEval * gamePhase)) / 3900;
						break;
				}
			}
		}
		return eval;
	}


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
		int startingAlpha = alpha;
		bool isInCheck = board.IsInCheck();
		bool notRoot = ply > 0;
		bool isPV = beta - alpha > 1;
		//return early statements.
		if (board.IsInCheckmate())
			return ply-999999;
		counters[^1]++;
		if (notRoot && (board.IsDraw()))
			return 0;
		if (isInCheck)
			depth = (depth < 0) ? (sbyte)1 : (sbyte)(depth + 1);

		if (depth <= 0)
		{
			if (isInCheck) Console.WriteLine("HEY");
			// Call Quiescence function at final depth
			return Quiescence(board, alpha, beta, color);
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

		ulong zobristHash = board.ZobristKey;
		// Transposition table lookup

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
		Move[] legalmoves = board.GetLegalMoves();
		Move goodMove = notRoot ? bestMove : overAllBestMove;
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
			if (timeToStop && !notRoot)
			{
				break;
			}

			board.MakeMove(move);

			// LMR: reduce the depth of the search for moves beyond a certain move count threshold
			int reduction = (int)((depth >= 4 && moveCount >= 4 && !board.IsInCheck() && !move.IsCapture && !move.IsPromotion && !isInCheck && !isPV) ? 1 + Math.Log2(depth) * Math.Log2(moveCount) / 2 : 0);
			//reduction -= isPV && reduction > 0 ? 1 : 0;
			int eval = -negamax(board, (sbyte)(depth - 1 - reduction), ply + 1, -beta, -alpha, -color);

			// Research with full depth if the move fails high - If it was better than allowed so opponnent wont play this branch.
			if (reduction > 0 && eval >= beta)
				eval = -negamax(board, (sbyte)(depth - 1), ply + 1, -beta, -alpha, -color);

			if (eval > max)
			{
				//if root level new best move is found, then save it to be played or for next iteration
				if (ply == 0)
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
				break;
			}
		}
		storeEntry(ref transposition, depth, alpha, beta, max, bestFoundMove, zobristHash);
		return max;
	}

	int Quiescence(Board board, int alpha, int beta, int color)
	{

		int standingPat = board.IsInCheck() ? -998999 : color * evaluation(board);

		if (standingPat >= beta)
		{
			return beta;
		}

		if (alpha < standingPat)
		{
			alpha = standingPat;
		}
		//**new code** For some reason seemed to worsen QSearch. So i'ts outcommented.
		/*ulong zobristHash = board.ZobristKey;

		ref Transposition transposition = ref transpositionTable[zobristHash & 0x7FFFFF];
		// Transposition table lookup
		Move bestMove = Move.NullMove;
		if (transposition.zobristHash == zobristHash)
		{
			lookups++;
			if (transposition.flag == 2) // lower bound
				alpha = Math.Max(alpha, transposition.evaluation);
			else // upper bound
				beta = Math.Min(beta, transposition.evaluation);

			//If we have an "exact" score (a < score < beta) just use that
			//if (transposition.flag == 1) return transposition.evaluation;
			//If we have a lower bound better than beta, use that
			//if (transposition.flag == 2 && transposition.evaluation >= beta) return transposition.evaluation;
			//If we have an upper bound worse than alpha, use that
			//if (transposition.flag == 3 && transposition.evaluation <= alpha) return transposition.evaluation;
			bestMove = transposition.move;
		}
		//**end**
		*/

		int oppositeColor = -1 * color;

		Move[] legalmoves = board.GetLegalMoves(true);
		Array.Sort(legalmoves, (a, b) => MoveOrderingHeuristic(b, board, Move.NullMove).CompareTo(MoveOrderingHeuristic(a, board, Move.NullMove)));
		int score = 0;
		//start searching
		foreach (Move move in legalmoves)
		{
			board.MakeMove(move);
			score = -Quiescence(board, -beta, -alpha, oppositeColor);
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
		return alpha;
	}


	void storeEntry(ref Transposition transposition, sbyte depth, int alpha, int beta, int bestEvaluation, Move bestMove, ulong zobristHash)
	{
		entryCount++;

		// Transposition table store
		transposition.evaluation = bestEvaluation;
		transposition.zobristHash = zobristHash;
		transposition.move = bestMove;
		if (bestEvaluation > alpha)
		{
			Console.WriteLine("exact");
			transposition.flag = 1;
		}//"exact" score
		else if (bestEvaluation >= beta)
		{
			Console.WriteLine("lower");
			transposition.flag = 2; //lower bound
		}
		else
		{
			transposition.flag = 3; //upper bound
			Console.WriteLine("upper");

		}
		transposition.depth = (sbyte)depth;
	}
}