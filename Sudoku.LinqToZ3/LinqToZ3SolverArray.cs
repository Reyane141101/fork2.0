using Sudoku.Shared;
using System.Collections;
using System.Text;
using Z3.LinqBinding;

namespace Z3.LinqBinding.Sudoku
{

	/// <summary>
	/// Class that represents a Sudoku, fully or partially completed.
	/// It holds a list of 81 int for cells, with 0 for empty cells
	/// Can parse strings and files from most common formats and displays the sudoku in an easy to read format
	/// </summary>
	public class SudokuAsArray
	{

		private static readonly int[] Indices = Enumerable.Range(0, 9).ToArray();

		// The List property makes it easier to manipulate cells,
		public List<int> Cells { get; set; } = Enumerable.Repeat(0, 81).ToList();

		/// <summary>
		/// Creates a Z3 theorem to solve the sudoku, adding the general constraints, and the mask constraints for this particular Sudoku
		/// </summary>
		/// <param name="context">The linq to Z3 context wrapping Z3</param>
		/// <returns>a theorem with all constraints compounded</returns>
		public Theorem<SudokuAsArray> CreateTheorem(Z3Context context)
		{
			var toReturn = Create(context);
			for (int i = 0; i < 81; i++)
			{
				if (Cells[i] != 0)
				{
					var idx = i;
					var cellValue = Cells[i];
					toReturn = toReturn.Where(sudoku => sudoku.Cells[idx] == cellValue);
				}
			}

			return toReturn;

		}


		/// <summary>
		/// Creates a Z3-capable theorem to solve a Sudoku
		/// </summary>
		/// <param name="context">The wrapping Z3 context used to interpret c# Lambda into Z3 constraints</param>
		/// <returns>A typed theorem to be further filtered with additional contraints</returns>
		public static Theorem<SudokuAsArray> Create(Z3Context context)
		{

			var sudokuTheorem = context.NewTheorem<SudokuAsArray>();

			// Cells have values between 1 and 9
			for (int i = 0; i < 9; i++)
			{
				for (int j = 0; j < 9; j++)
				{
					//To avoid side effects with lambdas, we copy indices to local variables
					var i1 = i;
					var j1 = j;
					sudokuTheorem = sudokuTheorem.Where(sudoku => (sudoku.Cells[i1 * 9 + j1] > 0 && sudoku.Cells[i1 * 9 + j1] < 10));
				}
			}

			// Rows must have distinct digits
			for (int r = 0; r < 9; r++)
			{
				//Again we avoid Lambda closure side effects
				var r1 = r;
				sudokuTheorem = sudokuTheorem.Where(t => Z3Methods.Distinct(Indices.Select(j => t.Cells[r1 * 9 + j]).ToArray()));

			}

			// Columns must have distinct digits
			for (int c = 0; c < 9; c++)
			{
				//Preventing closure side effects
				var c1 = c;
				sudokuTheorem = sudokuTheorem.Where(t => Z3Methods.Distinct(Indices.Select(i => t.Cells[i * 9 + c1]).ToArray()));
			}

			// Boxes must have distinct digits
			for (int b = 0; b < 9; b++)
			{
				//On évite les effets de bords par closure
				var b1 = b;
				// We retrieve to top left cell for all boxes, using integer division and remainders.
				var iStart = b1 / 3;
				var jStart = b1 % 3;
				var indexStart = iStart * 3 * 9 + jStart * 3;
				sudokuTheorem = sudokuTheorem.Where(t => Z3Methods.Distinct(new int[]
					  {
					 t.Cells[indexStart ],
					 t.Cells[indexStart+1],
					 t.Cells[indexStart+2],
					 t.Cells[indexStart+9],
					 t.Cells[indexStart+10],
					 t.Cells[indexStart+11],
					 t.Cells[indexStart+18],
					 t.Cells[indexStart+19],
					 t.Cells[indexStart+20],
					  }
				   )
				);
			}

			return sudokuTheorem;
		}


		/// <summary>
		/// Displays a Sudoku in an easy-to-read format
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			var lineSep = new string('-', 31);
			var blankSep = new string(' ', 8);

			var output = new StringBuilder();
			output.Append(lineSep);
			output.AppendLine();

			for (int row = 1; row <= 9; row++)
			{
				output.Append("| ");
				for (int column = 1; column <= 9; column++)
				{

					var value = Cells[(row - 1) * 9 + (column - 1)];

					output.Append(value);
					if (column % 3 == 0)
					{
						output.Append(" | ");
					}
					else
					{
						output.Append("  ");
					}
				}

				output.AppendLine();
				if (row % 3 == 0)
				{
					output.Append(lineSep);
				}
				else
				{
					output.Append("| ");
					for (int i = 0; i < 3; i++)
					{
						output.Append(blankSep);
						output.Append("| ");
					}
				}
				output.AppendLine();
			}

			return output.ToString();
		}


		public static SudokuAsArray ParseGrid(SudokuGrid s)
		{
			var cells = new List<int>(81);
			for (int row = 0; row < 9; row++)
			{
				for (int col = 0; col < 9; col++)
				{
					cells.Add(s.Cells[row][col]);
				}
			}
			var toReturn = new SudokuAsArray() { Cells = new List<int>(cells) };
			cells.Clear();
			return toReturn;
		}


		public string Export()
		{
			var output = new StringBuilder();

			for (int row = 1; row <= 9; row++)
			{
				for (int column = 1; column <= 9; column++)
				{
					var value = Cells[(row - 1) * 9 + (column - 1)];
					output.Append(value);
				}
			}

			return output.ToString();
		}
	}
}
namespace Sudoku.LinqToZ3
{
	public class LinqToZ3SolverArray : ISudokuSolver
	{
		public SudokuGrid Solve(SudokuGrid s)
		{
			var context = new Z3Context();
			Z3.LinqBinding.Sudoku.SudokuAsArray.Create(context);
			var grid = Z3.LinqBinding.Sudoku.SudokuAsArray.ParseGrid(s);
			var theorem = grid.CreateTheorem(context);
			var sudokuSolved = theorem.Solve();
			var toReturn = SudokuGrid.ReadSudoku(sudokuSolved.Export());
			return toReturn;
		}
	}
}