using System;
using System.Diagnostics;
using System.IO;
using ChessChallenge.API;
/// <summary>
/// Copy the code to MyBot.cs, rename it MyBot name, and that's it. Make sure that the exe is at the path.
/// </summary>
public class SebLague : IChessBot
{
	private Process sebLagueProcess;
	private StreamWriter Ins() => sebLagueProcess.StandardInput;
	private StreamReader Outs() => sebLagueProcess.StandardOutput;

	public SebLague()
	{
		var sebLagueBin = "\"C:\\Users\\jonat\\Downloads\\Coding-Adventure-Bot\\Coding-Adventure-Bot\\Chess-Coding-Adventure.exe\"";
		if (sebLagueBin == null)
		{
			throw new Exception("Missing environment variable: 'SEBLAGUE_BIN'");
		}
		sebLagueProcess = new();
		sebLagueProcess.StartInfo.RedirectStandardOutput = true;
		sebLagueProcess.StartInfo.RedirectStandardInput = true;
		sebLagueProcess.StartInfo.FileName = sebLagueBin;
		sebLagueProcess.Start();

		Ins().WriteLine("uci");
		string? line;
		var isOk = false;

		while ((line = Outs().ReadLine()) != null)
		{
			if (line == "uciok")
			{
				isOk = true;
				break;
			}
		}

		if (!isOk)
		{
			throw new Exception("Failed to communicate with SebLague");
		}

	}

	public Move Think(Board board, Timer timer)
	{
		Ins().WriteLine("ucinewgame");
		Ins().WriteLine($"position fen {board.GetFenString()}");
		var timeString = board.IsWhiteToMove ? "wtime" : "btime";
		Ins().WriteLine($"go {timeString} {timer.MillisecondsRemaining}");

		string? line;
		Move? move = null;

		while ((line = Outs().ReadLine()) != null)
		{
			if (line.StartsWith("bestmove"))
			{
				var moveStr = line.Split()[1];
				move = new Move(moveStr, board);

				break;
			}
		}

		if (move == null)
		{
			throw new Exception("Engine crashed");
		}

		return (Move)move;
	}
}