using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ReTestTBot
{
    public class ParseFormuls
    {
        public async Task<Func<double, double>> ParseFormulaSinAsync(string formulaText)
        {
            return await Task.Run(() =>
            {
                ParameterExpression x = Expression.Parameter(typeof(double), "x");

                // Заменяем sin и cos в формуле
                string modifiedFormula = formulaText.Replace("sin", "Math.Sin");

                // Парсим измененную формулу в динамическое выражение
                LambdaExpression e = DynamicExpressionParser.ParseLambda(new ParameterExpression[] { x }, null, modifiedFormula);

                // Компилируем выражение в функцию
                return (Func<double, double>)e.Compile();
            });
        }

        internal async Task<Func<double, double>> ParseFormulaCosAsync(string formulaText)
        {
            return await Task.Run(() =>
            {
                ParameterExpression x = Expression.Parameter(typeof(double), "x");

                // Заменяем sin и cos в формуле
                string modifiedFormula = formulaText.Replace("cos", "Math.Cos");

                // Парсим измененную формулу в динамическое выражение
                LambdaExpression e = DynamicExpressionParser.ParseLambda(new ParameterExpression[] { x }, null, modifiedFormula);

                // Компилируем выражение в функцию
                return (Func<double, double>)e.Compile();
            });
        }

        internal async Task<Func<double, double>> ParseFormulaTanAsync(string formulaText)
        {
            return await Task.Run(() =>
            {
                ParameterExpression x = Expression.Parameter(typeof(double), "x");

                // Заменяем sin и cos в формуле
                string modifiedFormula = formulaText.Replace("tan", "Math.Tan");

                // Парсим измененную формулу в динамическое выражение
                LambdaExpression e = DynamicExpressionParser.ParseLambda(new ParameterExpression[] { x }, null, modifiedFormula);

                // Компилируем выражение в функцию
                return (Func<double, double>)e.Compile();
            });
        }

        public async Task<Func<double, double>> ParseFormulaCotAsync(string formulaText)
        {
            return await Task.Run(() =>
            {
                ParameterExpression x = Expression.Parameter(typeof(double), "x");

                // Заменяем sin и cos в формуле
                string modifiedFormula = formulaText.Replace("cot", "1 / Math.Tan");

                // Парсим измененную формулу в динамическое выражение
                LambdaExpression e = DynamicExpressionParser.ParseLambda(new ParameterExpression[] { x }, null, modifiedFormula);

                // Компилируем выражение в функцию
                return (Func<double, double>)e.Compile();
            });
        }
    }
}
