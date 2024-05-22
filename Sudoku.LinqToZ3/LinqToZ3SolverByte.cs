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
	public class SudokuByte
	{

		private static readonly byte[] Indices = { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
		// The List property makes it easier to manipulate cells,
		public List<byte> Cells { get; set; } = Enumerable.Repeat((byte)0, (byte)81).ToList();

		/// <summary>
		/// Creates a Z3 theorem to solve the sudoku, adding the general constraints, and the mask constraints for this particular Sudoku
		/// </summary>
		/// <param name="context">The linq to Z3 context wrapping Z3</param>
		/// <returns>a theorem with all constraints compounded</returns>
		public Theorem<SudokuByte> CreateTheorem(Z3Context context)
		{
			var toReturn = Create(context);
			for (byte i = 0; i < 81; i++)
			{
				if (Cells[i] != 0)
				{
					byte idx = i;
					byte cellValue = Cells[i];
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
		public static Theorem<SudokuByte> Create(Z3Context context)
		{

			var sudokuTheorem = context.NewTheorem<SudokuByte>();

			
			// Cells have values between 1 and 9
			for (byte i = 0; i < 9; i++)
			{
				for (byte j = 0; j < 9; j++)
				{
					//To avoid side effects with lambdas, we copy indices to local variables
					var i1 = i;
					var j1 = j;
					sudokuTheorem = sudokuTheorem.Where(sudoku => (sudoku.Cells[i1 * 9 + j1] > 0 && sudoku.Cells[i1 * 9 + j1] < 10));
				}
			}

			// Rows must have distinct digits
			for (byte r = 0; r < 9; r++)
			{
				//Again we avoid Lambda closure side effects
				var r1 = r;
				sudokuTheorem = sudokuTheorem.Where(t => Z3Methods.Distinct(Indices.Select(j => t.Cells[r1 *  9 + j]).ToArray()));

			}

			// Columns must have distinct digits
			for (byte c = 0; c < 9; c++)
			{
				//Preventing closure side effects
				byte c1 = c;
				sudokuTheorem = sudokuTheorem.Where(t => Z3Methods.Distinct(Indices.Select(i => t.Cells[i * 9 + c1]).ToArray()));
			}

			// Boxes must have distinct digits

			for (byte b = 0; b < 9; b++)
			{
				//On évite les effets de bords par closure
				byte b1 = b;
				// Calculer le coin supérieur gauche de chaque boîte 3x3
				byte iStart = (byte) ((b1 / 3) * 3);
				byte jStart = (byte) ((b1 % 3) * 3);
				byte []indices = new byte[]
				{
					(byte)(iStart * 9 + jStart),
					(byte)(iStart * 9 + jStart + 1),
					(byte)(iStart * 9 + jStart + 2),
					(byte)((iStart + 1) * 9 + jStart),
					(byte)((iStart + 1) * 9 + jStart + 1),
					(byte)((iStart + 1) * 9 + jStart + 2),
					(byte)((iStart + 2) * 9 + jStart),
					(byte)((iStart + 2) * 9 + jStart + 1),
					(byte)((iStart + 2) * 9 + jStart + 2)
				};
				
				sudokuTheorem = sudokuTheorem.Where(t => Z3Methods.Distinct(indices.Select(idx => t.Cells[idx])));
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

			for (byte row = 1; row <= 9; row++)
			{
				output.Append("| ");
				for (byte column = 1; column <= 9; column++)
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
					for (byte i = 0; i < 3; i++)
					{
						output.Append(blankSep);
						output.Append("| ");
					}
				}
				output.AppendLine();
			}

			return output.ToString();
		}


		public static SudokuByte ParseGrid(SudokuGrid s)
		{
			var cells = new List<byte>(81);
			for (byte row = 0; row < 9; row++)
			{
				for (byte col = 0; col < 9; col++)
				{
					cells.Add((byte)s.Cells[row][col]);
				}
			}
			var toReturn = new SudokuByte() { Cells = new List<byte>(cells) };
			cells.Clear();
			return toReturn;
		}


		public string Export()
		{
			var output = new StringBuilder();

			for (byte row = 1; row <= 9; row++)
			{
				for (byte column = 1; column <= 9; column++)
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
	public class LinqToZ3SolverByte : ISudokuSolver
	{
		public SudokuGrid Solve(SudokuGrid s)
		{
			var context = new Z3Context();
			Z3.LinqBinding.Sudoku.SudokuByte.Create(context);
			var grid = Z3.LinqBinding.Sudoku.SudokuByte.ParseGrid(s);
			var theorem = grid.CreateTheorem(context);
			var sudokuSolved = theorem.Solve();
			var toReturn = SudokuGrid.ReadSudoku(sudokuSolved.Export());
			return toReturn;
		}
	}
}