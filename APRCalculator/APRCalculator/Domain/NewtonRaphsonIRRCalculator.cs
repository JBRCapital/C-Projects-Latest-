// NewtonRaphsonIRRCalculator.cs - Calculate the Internal rate of return for a given set of cashflows.
// Zainco Ltd
// Author: Joseph A. Nyirenda <joseph.nyirenda@gmail.com>
//             Mai Kalange<code5p@yahoo.co.uk>
// Copyright (c) 2008 Joseph A. Nyirenda, Mai Kalange, Zainco Ltd
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the GNU General Public
// License as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
//
// You should have received a copy of the GNU General Public
// License along with this program; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place - Suite 330,
// Boston, MA 02111-1307, USA.


using System;
using APRCalculator.Exceptions;
using System.Collections.Generic;

namespace APRCalculator.Domain
{
    public class NewtonRaphsonIRRCalculator : ICalculator
    {
        internal NewtonRaphsonIRRCalculator() 
        {
            Results = new List<KeyValuePair<double, double>>();
        }
        private int _numberOfIterations;
        private double _result;

        public List<double> CashFlows { get; set; }

        public static ICalculator Instance
        {
            get
            { return new NewtonRaphsonIRRCalculator(); }
        }
        /// <summary>
        /// Gets a value indicating whether this instance is valid cash flows.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is valid cash flows; otherwise, <c>false</c>.
        /// </value>
        public bool IsValidCashFlows
        {
            //Cash flows for the first period must be positive
            //There should be at least two cash flow periods         
            get
            {
                const int MIN_NO_CASH_FLOW_PERIODS = 2;

                if (CashFlows.Count < MIN_NO_CASH_FLOW_PERIODS || (CashFlows[0] > 0))
                {
                    throw new ArgumentOutOfRangeException(
                        "Cash flow for the first period  must be negative and there should");
                }
                return true;
            }
        }

        /// <summary>
        /// Gets the initial guess.
        /// </summary>
        /// <value>The initial guess.</value>
        private double InitialGuess
        {
            get
            {
                return  -1 * (1 + (CashFlows[1] / CashFlows[0]));
            }
        }

        #region ICalculator Members

        public double Execute()
        {
            if (IsValidCashFlows)
            {
                DoNewtonRapshonCalculation(InitialGuess);

                if (_result > 1)
                    throw new IRRCalculationException(
                        "Failed to calculate the IRR for the cash flow series. Please provide a valid cash flow sequence");
            }
            return _result;
        }

        private void RaiseEvent()
        {
            var dataPointGenerated = OnDataPointGenerated;
            if (dataPointGenerated != null)
                dataPointGenerated(this, new IRRCalculatorEventArgs(_result, _numberOfIterations));
        }

        public event EventHandler<IRRCalculatorEventArgs> OnDataPointGenerated;

        #endregion

        int maxIterations = 0;
        int MaxIterations { get {
            if (maxIterations == 0)
                {
                    maxIterations = ConfigurationHelper.MaxIterations;
                }
                return maxIterations;
        } }

        /// <summary>
        /// Does the newton rapshon calculation.
        /// </summary>
        /// <param name="estimatedReturn">The estimated return.</param>
        /// <returns></returns>
        private void DoNewtonRapshonCalculation(double estimatedReturn)
        {
            _numberOfIterations++;
            _result = estimatedReturn - SumOfIRRPolynomial(estimatedReturn)/IRRDerivativeSum(estimatedReturn);
            Results.Add(new KeyValuePair<double, double>(_numberOfIterations, _result));
          
            while (!HasConverged(_result) && MaxIterations != _numberOfIterations)
            {
                RaiseEvent();
                DoNewtonRapshonCalculation(_result);
            }
        }

        /// <summary>
        /// Sums the of IRR polynomial.
        /// </summary>
        /// <param name="estimatedReturnRate">The estimated return rate.</param>
        /// <returns></returns>
        private double SumOfIRRPolynomial(double estimatedReturnRate)
        {
            var sumOfPolynomial = 0d;
            if (IsValidIterationBounds(estimatedReturnRate))
                for (var j = 0; j < CashFlows.Count; j++)
                {
                    sumOfPolynomial += CashFlows[j] / (Math.Pow((1 + estimatedReturnRate), j));
                }
            return sumOfPolynomial;
        }


        double tolerance = 0;
        double Tolerance
        {
            get
            {
                if (tolerance == 0)
                {
                    tolerance = ConfigurationHelper.Tolerance;
                }
                return tolerance;
            }
        }

        /// <summary>
        /// Determines whether the specified estimated return rate has converged.
        /// </summary>
        /// <param name="estimatedReturnRate">The estimated return rate.</param>
        /// <returns>
        /// 	<c>true</c> if the specified estimated return rate has converged; otherwise, <c>false</c>.
        /// </returns>
        private bool HasConverged(double estimatedReturnRate)
        {
            //Check that the calculated value makes the IRR polynomial zero.
            var isWithinTolerance = Math.Abs(SumOfIRRPolynomial(estimatedReturnRate)) <= Tolerance;
            return (isWithinTolerance) ? true : false;
        }

        /// <summary>
        /// IRRs the derivative sum.
        /// </summary>
        /// <param name="estimatedReturnRate">The estimated return rate.</param>
        /// <returns></returns>
        private double IRRDerivativeSum(double estimatedReturnRate)
        {
            var sumOfDerivative = 0d;
            if (IsValidIterationBounds(estimatedReturnRate))
                for (var i = 1; i < CashFlows.Count; i++)
                {
                    sumOfDerivative += CashFlows[i] * (i) / Math.Pow((1 + estimatedReturnRate), i);
                }
            return sumOfDerivative * -1;
        }

        /// <summary>
        /// Determines whether [is valid iteration bounds] [the specified estimated return rate].
        /// </summary>
        /// <param name="estimatedReturnRate">The estimated return rate.</param>
        /// <returns>
        /// 	<c>true</c> if [is valid iteration bounds] [the specified estimated return rate]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsValidIterationBounds(double estimatedReturnRate)
        {
            return estimatedReturnRate != -1 && (estimatedReturnRate < int.MaxValue) &&
                   (estimatedReturnRate > int.MinValue);
        }

        public List<KeyValuePair<double, double>> Results
        {
            get;set;
        }
    }
}