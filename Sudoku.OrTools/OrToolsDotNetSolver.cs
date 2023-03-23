﻿using Sudoku.Shared;

using Google.OrTools.Sat;
using IntVar = Google.OrTools.Sat.IntVar;

namespace Sudoku.OrTools
{
    public class OrToolsDotNetSolver : ISudokuSolver
    {
        // https://developers.google.com/optimization/cp
        // https://developers.google.com/optimization/cp/cp_solver

        private CpModel _model = new CpModel();

        private const int GridSize = 9;

        public SudokuGrid Solve(SudokuGrid s)
        {
            // Create all variables and constants of the sudoku grid
            List<List<IntVar>> gridVars = new List<List<IntVar>>();
            
            for (int i = 0; i < GridSize; i++)
            {
                gridVars.Add(new List<IntVar>());
                for (int j = 0; j < GridSize; j++)
                {
                    if (s.Cells[i][j] == 0)
                    {
                        gridVars[i].Add(_model.NewIntVar(1, 9, $"{i}{j}"));
                    }
                    else
                    {
                        gridVars[i].Add(_model.NewConstant(s.Cells[i][j]));
                    }
                }
            }

            // Add constraints
            for (int i = 0; i < GridSize; i++)
            {
                
                // Columns and Rows constaints
                List<IntVar> col = new List<IntVar>();
                List<IntVar> row = new List<IntVar>();

                for (int j = 0; j < GridSize; j++)
                {
                    row.Add(gridVars[i][j]); 
                    col.Add(gridVars[j][i]); 
                    
                    if (i % 3 == 1 && j % 3 == 1) // if we are at the center of a square
                    {
                        // Square constraints
                        List<IntVar> square = new List<IntVar>();

                        for (int k = i - 1; k < i + 2; k++)
                        {
                            for (int l = j - 1; l < j + 2; l++)
                            {
                                square.Add(gridVars[k][l]);
                            }
                        }

                        _model.AddAllDifferent(square);
                    }
                }

                _model.AddAllDifferent(col);
                _model.AddAllDifferent(row);
            }

            CpSolver solver = new CpSolver();
            CpSolverStatus status = solver.Solve(_model);
            
            if (status is CpSolverStatus.Feasible or CpSolverStatus.Optimal)
            {
                for (int i = 0; i < GridSize; i++)
                {
                    for (int j = 0; j < GridSize; j++)
                    {
                        s.Cells[i][j] = (int)solver.Value(gridVars[i][j]);
                    }
                }
            }
            
            return s;
        }
    }
}
