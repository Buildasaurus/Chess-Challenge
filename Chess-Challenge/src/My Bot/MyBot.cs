using ChessChallenge.API;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Numerics;

/// <summary>
/// 
/// Ideas
/// - Kind bot - When winning - go for a draw instead 
/// - No queen bot - just throw it away and try to win anyways
/// - scholars mate / trick bot - always go for certain tricks.
/// </summary>
public class MyBot : IChessBot
{

	List<int> counters = new List<int>(); //#DEBUG

	private readonly decimal[] PackedPestoTables = {
			63746705523041458768562654720m, 71818693703096985528394040064m, 75532537544690978830456252672m, 75536154932036771593352371712m, 76774085526445040292133284352m, 3110608541636285947269332480m, 936945638387574698250991104m, 75531285965747665584902616832m,
			77047302762000299964198997571m, 3730792265775293618620982364m, 3121489077029470166123295018m, 3747712412930601838683035969m, 3763381335243474116535455791m, 8067176012614548496052660822m, 4977175895537975520060507415m, 2475894077091727551177487608m,
			2458978764687427073924784380m, 3718684080556872886692423941m, 4959037324412353051075877138m, 3135972447545098299460234261m, 4371494653131335197311645996m, 9624249097030609585804826662m, 9301461106541282841985626641m, 2793818196182115168911564530m,
			77683174186957799541255830262m, 4660418590176711545920359433m, 4971145620211324499469864196m, 5608211711321183125202150414m, 5617883191736004891949734160m, 7150801075091790966455611144m, 5619082524459738931006868492m, 649197923531967450704711664m,
			75809334407291469990832437230m, 78322691297526401047122740223m, 4348529951871323093202439165m, 4990460191572192980035045640m, 5597312470813537077508379404m, 4980755617409140165251173636m, 1890741055734852330174483975m, 76772801025035254361275759599m,
			75502243563200070682362835182m, 78896921543467230670583692029m, 2489164206166677455700101373m, 4338830174078735659125311481m, 4960199192571758553533648130m, 3420013420025511569771334658m, 1557077491473974933188251927m, 77376040767919248347203368440m,
			73949978050619586491881614568m, 77043619187199676893167803647m, 1212557245150259869494540530m, 3081561358716686153294085872m, 3392217589357453836837847030m, 1219782446916489227407330320m, 78580145051212187267589731866m, 75798434925965430405537592305m,
			68369566912511282590874449920m, 72396532057599326246617936384m, 75186737388538008131054524416m, 77027917484951889231108827392m, 73655004947793353634062267392m, 76417372019396591550492896512m, 74568981255592060493492515584m, 70529879645288096380279255040m,
		};
	int[][] UnpackedPestoTables;



	/*void printtables()
	{
		Console.WriteLine(UnpackedPestoTables.ToString());
		for(int i = 0; i < 12; i++)
		{

			Console.WriteLine($"table for piecetype {i}");
			string builder = "";
			for (int j = 0; j < 64; j++)
			{
				builder += " " + UnpackedPestoTables[j][i];
				if (j % 8 == 7)
				{ 
					Console.WriteLine(builder); 
					builder = "";
				}
			}
		}
	}*/
	// History table definition
	int[,,] historyTable;
	// Define a structure to store transposition table entries
	int lookups = 0; //#DEBUG
	int entryCount = 0;//#DEBUG
	int startime;//#DEBUG

	private readonly int[] moveScores = new int[218], phase_weight = { 0, 1, 1, 2, 4, 0 },
				PieceValues = { 82, 337, 365, 477, 1025, 0, // Middlegame
									94, 281, 297, 512, 936, 0}; // Endgame

	// Create a transposition table // key, move, score/eval, depth, flag.
	private readonly (ulong, Move, int, sbyte, byte)[] transpositionTable = new (ulong, Move, int, sbyte, byte)[0x400000];

	bool timeToStop;

	Move overAllBestMove;
	ChessChallenge.API.Timer timer;
	int timeForTurn;
	public Move Think(Board board, ChessChallenge.API.Timer _timer)
	{
		if (board.GameMoveHistory.Length < 2)
			UnpackedPestoTables = PackedPestoTables.Select(packedTable =>
			{
				int pieceType = 0;
				return decimal.GetBits(packedTable).Take(3)
					.SelectMany(c => BitConverter.GetBytes(c)
						.Select(square => (int)((sbyte)square * 1.461) + PieceValues[pieceType++]))
					.ToArray();
			}).ToArray();

		Console.WriteLine("-----My bot thinking----");//#DEBUG
													  //killerMoves.Clear();
		timer = _timer;
		historyTable = new int[2, 7, 64];
		timeForTurn = Math.Min(timer.MillisecondsRemaining - 50, timer.MillisecondsRemaining / 30);
		timeToStop = false;
		lookups = 0; //#DEBUG
		entryCount = 0; //#DEBUG
		counters.Add(0);//#DEBUG
		int bestEval = 0; //#DEBUG
		int thinkStart = timer.MillisecondsRemaining; //#DEBUG
		for (sbyte d = 1; d <= 32; d++)
		{
			if (timeToStop) break;
			startime = timer.MillisecondsRemaining; //#DEBUG

			bestEval = -negamax(board, d, 0, -10000000, 10000000, board.IsWhiteToMove ? 1 : -1);
			Console.WriteLine($"info string best move at depth {d} was {overAllBestMove} with eval at {bestEval}");//#DEBUG
			Console.WriteLine($"info string Time used for depth {d}: {startime - timer.MillisecondsRemaining} miliseconds");//#DEBUG
			Console.WriteLine("info string -------node count------- " + counters[^1]);//#DEBUG

		}

		Console.WriteLine("info string -------node count------- " + counters[^1]);//#DEBUG
		Console.WriteLine("info string useful lookups:  " + lookups);//#DEBUG
		Console.WriteLine("info string Entry count " + entryCount);//#DEBUG
		Console.WriteLine($"info string Final best move was {overAllBestMove} with eval at {bestEval}");//#DEBUG
		Console.WriteLine($"info string Time used for completed search: {thinkStart - timer.MillisecondsRemaining} miliseconds");//#DEBUG

		//if (overAllBestMove == Move.NullMove) overAllBestMove = board.GetLegalMoves()[0]; // just in case there basically is no time.

		return overAllBestMove;
	}



	//There are 3 tokens to save here, using fewer variables, and negating eval in each loop
	int evaluation(Board board)
	{
		int middlegame = 0, endgame = 0, gamephase = 0, sideToMove = 2;
		for (; --sideToMove >= 0;)
		{
			for (int piece = -1, square; ++piece < 6;)
				for (ulong mask = board.GetPieceBitboard((PieceType)piece + 1, sideToMove > 0); mask != 0;)
				{
					//gamephase goes from 0 to 24, 24 is midgame, 0 is endgame
					gamephase += phase_weight[piece];
					//Get index of first bit, which is index of the piece- Then XOR if it's white, to flip the board,
					//cuz tables are opposite, so 0,0 isn't a1, but a8.
					square = BitboardHelper.ClearAndGetIndexOfLSB(ref mask) ^ 56 * sideToMove;
					middlegame += UnpackedPestoTables[square][piece];
					endgame += UnpackedPestoTables[square][piece + 6];
				}

			middlegame *= -1;
			endgame *= -1;
		}
		return (middlegame * gamephase + endgame * (24 - gamephase)) / 24;
	}


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
		depth = Math.Max(depth, (sbyte)0);
		//start searching

		int oldAlpha = alpha, R = 2, movesScored = 0, moveCount = 0, max = -100000000;

		if (depth < 0) Console.WriteLine("smaller than 0"); //#DEBUG
															//Much used variables
		bool isPV = beta - alpha > 1, notRoot = ply > 0, isInCheck = board.IsInCheck();

		//Draw detection
		if (notRoot && board.IsDraw())
			return 0; //slight discouragement of draws.
		if (board.IsInCheckmate())
			return ply - 999999;

		//Debug
		counters[^1]++; //#DEBUG



		//check extensions - MUST BE BEFORE QSEARCH	
		if (isInCheck)
			depth++;

		//QSearch
		bool isQSearch = depth <= 0;
		if (isQSearch)
		{
			max = color * evaluation(board); 

			if (max >= beta)
				return max;
			alpha = Math.Max(alpha, max);
		}

		// Null move pruning - is perhaps best with more time on the clock?
		if (!isPV && !isInCheck && depth > R)
		{
			board.TrySkipTurn();
			int score = -negamax(board, (sbyte)(depth - R - 1), ply + 1, -beta, -alpha, -color);
			board.UndoSkipTurn();
			if (score >= beta)
				return score;
		}
		// Transposition table lookup
		ulong zobristHash = board.ZobristKey;
		ref var entry = ref transpositionTable[zobristHash & 0x3FFFFF];
		int entryScore = entry.Item3, entryFlag = entry.Item5;

		//If we have an "exact" score (a < score < beta) just use that
		//If we have a lower bound better than beta, use that
		//If we have an upper bound worse than alpha, use that
		if (notRoot && entry.Item1 == zobristHash && entry.Item4 >= depth && Math.Abs(entryScore) < 50000 && (
				// Exact
				entryFlag == 1 ||
				// Upperbound
				entryFlag == 2 && entryScore <= alpha ||
				// Lowerbound
				entryFlag == 3 && entryScore >= beta))
		{
			lookups++; //#DEBUG
			return entryScore;
		}


		if (!notRoot) Console.WriteLine($"info string Bestmove at depth{depth} was for a starter: {overAllBestMove}");//#DEBUG

		// Generate legal moves and sort them
		Move goodMove = notRoot ? entry.Item2 : overAllBestMove, bestFoundMove = Move.NullMove;

		// Gamestate, checkmate and draws
		Span<Move> legalmoves = stackalloc Move[218];
		board.GetLegalMovesNonAlloc(ref legalmoves, isQSearch);

		foreach (Move move in legalmoves)
			moveScores[movesScored++] = -(
			// Hash move
			move == goodMove ? 9_000_000 :
			// MVVLVA
			move.IsCapture ? 1_000_000 * (int)move.CapturePieceType - (int)move.MovePieceType :
			// History
			historyTable[board.IsWhiteToMove ? 0 : 1, (int)move.MovePieceType, move.TargetSquare.Index]);

		moveScores.AsSpan(0, legalmoves.Length).Sort(legalmoves);

		// if we are at root level, make sure that the overallbest move from earlier iterations is at top.
		foreach (Move move in legalmoves)
		{
			int eval;
			moveCount++; // Increment the move counter

			//Early stop at top level
			if (!timeToStop && timer.MillisecondsElapsedThisTurn > timeForTurn) timeToStop = true;
			if (timeToStop)
				return 0;

			board.MakeMove(move);
			int search(int reductions, int betas) => -negamax(board, (sbyte)(depth - 1 - reductions), ply + 1, -betas, -alpha, -color);
			if (isQSearch)
			{
				eval = search(depth, beta);
			}
			else
			{
				// LMR: reduce the depth of the search for moves beyond a certain move count threshold - Can save few tokens here with simpler reduction
				int reduction = (int)((depth >= 4 && moveCount >= 4 && !move.IsCapture && !move.IsPromotion && !isInCheck && !isPV) ? 1 + Math.Log2(depth) * Math.Log2(moveCount) / 2 : 0);
				//reduction = isPV && reduction > 0 ? 1 : 0;
				//local search function to save tokens.

				if (moveCount == 1)
					eval = search(reduction, beta);
				else
				{
					eval = search(reduction, alpha + 1); //should be +, because it will be negated in the search method.
					if (eval > alpha || (reduction > 0 && beta > eval))
						eval = search(0, beta);
				}


			}
			if (eval > max)
			{
				//if root level new best move is found, then save it to be played or for next iteration 
				if (ply == 0 && !timeToStop)
				{
					overAllBestMove = move;
					Console.WriteLine($"info string new Overall Best move: {move}");//#DEBUG
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


		// Transposition table store
		entryCount++; //#DEBUG
					  // Transposition table insertion
		entry = new(
			zobristHash,
			bestFoundMove == default ? entry.Item2 : bestFoundMove,
			max,
			depth,
			(byte)(max >= beta ? 3 : max <= oldAlpha ? 2 : 1));

		return max;
	}
}