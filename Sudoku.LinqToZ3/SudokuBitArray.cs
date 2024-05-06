using System.Collections;
using System.Text;
using Sudoku.Shared;
using Z3.LinqBinding;
class SudokuBitArray
{
    private BitArray[][] Grid;
    private static readonly int[] Indices = Enumerable.Range(0, 9).ToArray();


    public SudokuBitArray(SudokuGrid s)
    {
        // Initialiser la matrice de BitArray
        Grid = new BitArray[9][];
        for (int i = 0; i < 9; i++)
        {
            Grid[i] = new BitArray[9];
            for (int j = 0; j < 9; j++)
            {
                Grid[i][j] = new BitArray(10,false);
            }
        }
        ParseGrid(s); // On définis les valeurs dans notre grid ici
    }

    // Méthode pour définir un chiffre dans une cellule spécifique
    public void SetCell(int row, int col, int digit)
    {
        if (row >= 0 && row < 9 && col >= 0 && col < 9)
        {
            Grid[row][col].Set(digit, true);
        }
        else
        {
            throw new ArgumentException("La ligne et la colonne doivent être dans la plage de 0 à 8, et le chiffre doit être entre 1 et 9.");
        }
    }

    // Méthode pour vérifier si un chiffre est présent dans une cellule spécifique REMAR
    // REMARQUE CETTE METHODE EST USELESS CAR Z3.LINQBIDING NE SUPPORTE PAS LES GETTER commé
    public int get_cell(int row, int col)
    {
        if (row >= 0 && row < 9 && col >= 0 && col < 9)
        {
            for (int digit = 0; digit < 9; digit+=1)
            {
                if (Grid[row][col].Get(digit))
                {
                    return digit + 1;
                }
            }
            return 0;
        }
        else
        {
            throw new ArgumentException("La ligne et la colonne doivent être dans la plage de 0 à 8, et le chiffre doit être entre 1 et 9.");
        }
    }

 public int ValueCell(int row, int col)
    {
        if (row >= 0 && row < 9 && col >= 0 && col < 9)
        {
            for (int digit = 0; digit < 9; digit+=1)
            {
                if (Grid[row][col].Get(digit))
                {
                    return digit + 1;
                }
            }
            return 0;
        }
        else
        {
            throw new ArgumentException("La ligne et la colonne doivent être dans la plage de 0 à 8, et le chiffre doit être entre 1 et 9.");
        }
    }
    public BitArray this[int row, int col]
    {
        get
        {
            return this.Grid[row][col];
        }
    }


    public void ParseGrid(SudokuGrid s)
	{
			for (int row = 0; row < 9; row++)
			{
				for (int col = 0; col < 9; col++)
				{
                    SetCell(row,col,s.Cells[row][col]);
				}
			}
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

					var value = get_cell(row,column);

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

        public string Export()
		{
			var output = new StringBuilder();

			for (int row = 1; row <= 9; row++)
			{
				for (int column = 1; column <= 9; column++)
				{
					var value = get_cell(row,column);
					output.Append(value);
				}
			}

			return output.ToString();
		}
        //--------------------------------- AJOUT DE CONTRAINTEs ------------------------
        public Theorem<SudokuBitArray> CreateTheorem(Z3Context context)
		{
			var toReturn = Create(context);
			for (int i = 0; i < 9; i++)
			{
                for(int j = 0; j < 9;j++)
                {
                    if (get_cell(i,j)!=0)
				{
					var idx = i;
                    var jdx = j;
					var cellValue = get_cell(i,j);
					toReturn = toReturn.Where(sudoku => sudoku.Grid[idx][jdx][cellValue] == true );
				}
                }
			}

			return toReturn;

		}

        public static Theorem<SudokuBitArray> Create(Z3Context context)
		{

			var sudokuTheorem = context.NewTheorem<SudokuBitArray>();
            
			// Cells have values between 1 and 9
			for (int i = 0; i < 9; i++)
			{
				for (int j = 0; j < 9; j++)
				{
					//To avoid side effects with lambdas, we copy indices to local variables
					var i1 = i;
					var j1 = j;
					sudokuTheorem = sudokuTheorem.Where(
                        sudoku => sudoku.get_cell(i1,j1) > 0 && sudoku.get_cell(i1,j1) < 10);
				}
			}
            
			// Rows must have distinct digits
			for (int r = 0; r < 9; r++)
			{
				//Again we avoid Lambda closure side effects
				var r1 = r;
				sudokuTheorem = sudokuTheorem.Where(t => Z3Methods.Distinct(Indices.Select(j => t[r1,j])));

			}
            
			// Columns must have distinct digits
			for (int c = 0; c < 9; c++)
			{
				//Preventing closure side effects
				var c1 = c;
				sudokuTheorem = sudokuTheorem.Where(t => Z3Methods.Distinct(Indices.Select(i => t[c1,i])));
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
                        t.get_cell(b1,indexStart),
                        t.get_cell(b1,indexStart+1),
                        t.get_cell(b1,indexStart+2),
                        t.get_cell(b1,indexStart+9),
                        t.get_cell(b1,indexStart+10),
                        t.get_cell(b1,indexStart+11),
                        t.get_cell(b1,indexStart+18),
                        t.get_cell(b1,indexStart+20)
					  }
				   )
				);
			}
            
			return sudokuTheorem;
		}
}
