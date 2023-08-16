using ChessChallenge.API;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System;



namespace ChessChallenge.Example
{

	public class EvilBot : IChessBot
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
			Console.WriteLine("-----Evil bot thinking----");
			//killerMoves.Clear();
			timer = _timer;
			endMiliseconds = (int)Math.Ceiling(timer.MillisecondsRemaining * 0.975f);
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
			for (sbyte d = 1; d <= 32; d++)
			{
				if (timeToStop) break;
				startime = timer.MillisecondsRemaining;

				bestEval = -negamax(board, d, 0, -10000000, 10000000, color, 0);
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
				PieceList pieces = board.GetPieceList((PieceType)(i + 1), color);
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
		Transposition[] transpositionTable = new Transposition[0x7FFFFF + 1];
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
		int negamax(Board board, sbyte depth, int ply, int alpha, int beta, int color, int numExtensions)
		{
			bool notRoot = ply > 0;

			//return early statements.
			if (board.IsInCheckmate())
				return -999999;
			counters[^1]++;
			if (notRoot && (board.IsRepeatedPosition() || board.IsDraw() || board.IsInStalemate()))
				return 0;
			if (depth == 0)
			{
				// Call Quiescence function here
				return Quiescence(board, depth, alpha, beta, color);
			}


			ulong zobristHash = board.ZobristKey;

			ref Transposition transposition = ref transpositionTable[zobristHash & 0x7FFFFF];
			// Transposition table lookup
			Move bestMove = Move.NullMove;
			if (transposition.zobristHash == zobristHash && transposition.depth >= depth)
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

			if (!notRoot) Console.WriteLine($"info string Bestmove at depth{depth} was for a starter: {overAllBestMove}");


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
				//sbyte extension = board.IsInCheck() && numExtensions < 10 ? (sbyte)1 : (sbyte)0;

				int eval = -negamax(board, (sbyte)(depth - 1), ply + 1, -beta, -alpha, -color, numExtensions);

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
				if (alpha >= beta && notRoot) //alpha > beta shouldn't be possible at root, but anyways.
				{
					storeEntry(ref transposition, depth, alpha, beta, max, bestFoundMove, zobristHash);

					return max;
				}
			}
			storeEntry(ref transposition, depth, alpha, beta, max, bestFoundMove, zobristHash);
			return max;
		}
		int Quiescence(Board board, sbyte depth, int alpha, int beta, int color)
		{

			int standingPat = color * evaluation(board);

			if (standingPat >= beta)
			{
				return beta;
			}

			if (alpha < standingPat)
			{
				alpha = standingPat;
			}
			ulong zobristHash = board.ZobristKey;


			int oppositeColor = -1 * color;

			Move[] legalmoves = board.GetLegalMoves(true);
			Array.Sort(legalmoves, (a, b) => MoveOrderingHeuristic(b, board).CompareTo(MoveOrderingHeuristic(a, board)));
			int score = 0;

			Move bestFoundMove = Move.NullMove;

			//start searching
			foreach (Move move in legalmoves)
			{
				if (!move.IsCapture)
				{
					continue;
				}

				board.MakeMove(move);
				score = -Quiescence(board, (sbyte)(depth - 1), -beta, -alpha, oppositeColor);
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
			if (bestEvaluation < alpha)
				transposition.flag = 3; //upper bound
			else if (bestEvaluation >= beta)
			{
				transposition.flag = 2; //lower bound
			}
			else transposition.flag = 1; //"exact" score
			transposition.depth = (sbyte)depth;
		}
	}
}