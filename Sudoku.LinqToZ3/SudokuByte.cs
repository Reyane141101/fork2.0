using System;
using System.Collections;
using System.Text;
using Sudoku.Shared;
using Z3.LinqBinding;
class SudokuByte
{
    public int[] Cells;
	private static readonly int[] Indices = Enumerable.Range(0, 9).Select(i => (int)i).ToArray();



    public SudokuByte(SudokuGrid s)
    {
        // Initialiser la matrice de BitArray
       	Cells = new int[81];
    	ParseGrid(s); // On définis les valeurs dans notreCells ici
    }

    // Méthode pour définir un chiffre dans une cellule spécifique
    public void SetCell(int pos, int digit)
    {
        if (pos >=0 && pos < 81)
        {
           Cells[pos] =(int) digit; //Convertion en int pour le type .NET
        }
        else
        {
            throw new ArgumentException("La ligne et la colonne doivent être dans la plage de 0 à 8, et le chiffre doit être entre 1 et 9.");
        }
    }

    // Méthode pour vérifier si un chiffre est présent dans une cellule spécifique REMAR
    // REMARQUE CETTE METHODE EST USELESS CAR Z3.LINQBIDING NE SUPPORTE PAS LES GETTER 



    public void ParseGrid(SudokuGrid s)
	{
			for (int row = 0; row < 9; row++)
			{
				for (int col = 0; col < 9; col++)
				{
					int pos = row * 9 + col;
                    SetCell(pos,s.Cells[row][col]);
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

					var value = Cells[(row-1) *9 + (column - 1)];

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

			for (int row = 0; row <= 9; row++)
			{
				for (int column = 0; column <= 9; column++)
				{
					var value = Cells[row * 9 + column];
					output.Append(value);
				}
			}

			return output.ToString();
		}
        //--------------------------------- AJOUT DE CONTRAINTEs ------------------------
        public Theorem<SudokuByte> CreateTheorem(Z3Context context)
		{
			var toReturn = Create(context);
			for (int i = 0; i < 81; i++)
			{

                if (Cells[i]!=0)
				{
					var idx = i;
					var cellValue = Cells[i];
					toReturn = toReturn.Where(sudoku => sudoku.Cells[idx] == cellValue );
				}
			}

			return toReturn;

		}

        public static Theorem<SudokuByte> Create(Z3Context context)
		{

			var sudokuTheorem = context.NewTheorem<SudokuByte>();
            
			/*
			// Cells have values between 1 and 9
			for (int i = 0; i < 9; i++)
			{
				for (int j = 0; j < 9; j++)
				{
					//To avoid side effects with lambdas, we copy indices to local variables
					int i1 = (int)i;
					int j1 = (int)j;
					int minValue = 1;
    				int maxValue = 9;
					int index = 9;
					sudokuTheorem = sudokuTheorem.Where(
                        sudoku => sudoku.Cells[i1 * index + j1] > minValue && sudoku.Cells[i1 * index  + j1] < maxValue);
				}
			}
            
			// Rows must have distinct digits
			for (int r = 0; r < 9; r++)
			{
				//Again we avoid Lambda closure side effects
				int r1 = (int)r;
				int inter = 9;
				
				sudokuTheorem = sudokuTheorem.Where(t => Z3Methods.Distinct(Indices.Select(j => t.Cells[r1 * inter + j])));

			}
            
			// Columns must have distinct digits
			for (int c = 0; c < 9; c++)
			{
				//Preventing closure side effects
				var c1 = c;
				sudokuTheorem = sudokuTheorem.Where(t => Z3Methods.Distinct(Indices.Select(i => t.Cells[i * 9 + c1])));
			}


			// Boxes must have distinct digits
            for (int b = 0; b < 9; b++)
			{
				//On évite les effets de bords par closure
				var b1 = b;
				// Calculer le coin supérieur gauche de chaque boîte 3x3
				var iStart = (b1 / 3) * 3;
				var jStart = (b1 % 3) * 3;
				var indices = new int[]
				{
					iStart * 9 + jStart,
					iStart * 9 + jStart + 1,
					iStart * 9 + jStart + 2,
					(iStart + 1) * 9 + jStart,
					(iStart + 1) * 9 + jStart + 1,
					(iStart + 1) * 9 + jStart + 2,
					(iStart + 2) * 9 + jStart,
					(iStart + 2) * 9 + jStart + 1,
					(iStart + 2) * 9 + jStart + 2
				};
				
				sudokuTheorem = sudokuTheorem.Where(t => Z3Methods.Distinct(indices.Select(idx => t.Cells[idx])));
			}
			
			for (int i = 0; i < 81; i++)
    		{
        	// Ajouter une condition pour chaque cellule : cell >= 0
				int i1 = (int) i;
				int cmp = 0;
        		sudokuTheorem = sudokuTheorem.Where(sudoku => sudoku.Cells[i1] >= cmp);
    		}*/
            
			return sudokuTheorem;
		}
}
