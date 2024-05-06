using Sudoku.Shared;
using System.Collections;
using System.Text;
using Z3.LinqBinding;
using Sudoku.LinqToZ3;

namespace Sudoku.LinqToZ3
{
	public class LinqToZ3SolverBitArray : ISudokuSolver
	{
		public SudokuGrid Solve(SudokuGrid s)
		{
			var context = new Z3Context();
			var grid = new SudokuBitArray(s);
			var theorem = grid.CreateTheorem(context);
			var sudokuSolved = theorem.Solve();
			var toReturn = SudokuGrid.ReadSudoku(sudokuSolved.Export());
			return toReturn;
		}
	}
}