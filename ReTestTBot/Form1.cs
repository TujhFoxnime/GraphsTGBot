using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telegram.Bot;
using Telegram.Bot.Types;
using ZedGraph;
using System.Linq.Expressions;
using System.Linq.Dynamic.Core;
using ScottPlot.Renderable;
using System.Diagnostics;

namespace ReTestTBot
{
    public partial class Form1 : Form
    {
        private static TelegramBotClient BotClient;
        private ZedGraphControl zedGraphControl;
        private static Form1 instance;
        public Form1()
        {
            InitializeComponent();
            instance = this;
        }

        private void InitializeGraph(string userFormula)
        {
            ZedGraphControl zedGraphControl = new ZedGraphControl { Dock = DockStyle.Fill };

            // Создание графика
            GraphPane graphPane = zedGraphControl.GraphPane;
            graphPane.Title.Text = "График функции";
            graphPane.XAxis.Title.Text = "X";
            graphPane.YAxis.Title.Text = "Y";

            // Конвертация текста в функцию
            Func<double, double> function = GetFunctionFromText(userFormula);

            // Задание функции для построения
            PointPairList pointPairList = new PointPairList();
            double minX = -10;
            double maxX = 10;
            double step = 0.1;

            for (double x = minX; x <= maxX; x += step)
            {
                double y = function(x);
                pointPairList.Add(x, y);
            }

            // Добавление кривой на график
            LineItem curve = graphPane.AddCurve("Функция", pointPairList, System.Drawing.Color.Blue, SymbolType.None);

            // Обновление графика
            zedGraphControl.AxisChange();
            zedGraphControl.Invalidate();

        }

        private Func<double, double> GetFunctionFromText(string formulaText)
        {
            try
            {
                // Создаем параметр для выражения
                //ParameterExpression parameter = Expression.Parameter(typeof(double), "x");

                ParameterExpression x = Expression.Parameter(typeof(int), "x");
                ParameterExpression y = Expression.Parameter(typeof(int), "y");
                LambdaExpression e = DynamicExpressionParser.ParseLambda(new ParameterExpression[] { x, y }, null, formulaText);
                //Expression<Func<double, double>> expression = DynamicExpressionParser.ParseLambda<Func<double, double>>(new ParameterExpression[] (formulaText, parameter));

                // Компилируем выражение в функцию
                Func<double, double> function = (Func<double, double>)e.Compile();

                return function;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обработке формулы: {ex.Message}");
                return x => 0;
            }
        }

        static void Main(string[] args)
        {
            // Вызываем SetCompatibleTextRenderingDefault до создания первого объекта IWin32Window
            Application.SetCompatibleTextRenderingDefault(false);

            Form1 form = new Form1();
            form.RunBot();
        }

        public void RunBot()
        {
            BotClient = new TelegramBotClient("6820445732:AAHzvCAkkuFVYO7siXIJR5nppkNDWdUV-j4");

            BotClient.StartReceiving(Update, Error);
            Application.EnableVisualStyles();
            Application.Run(this); // Замените Application.Run(new Form1()) на Application.Run(this)
        }

        private static async Task Update(ITelegramBotClient client, Update update, System.Threading.CancellationToken token)
        {
            var message = update.Message;

            string command = update.Message.Text.ToLower();

            try
            {
                
                switch (command)
                {
                    case "/start":
                        var welcomeMessage = "Добро пожаловать! Я ваш бот." +
                            "\nПеред вводом выражения рекомендуется ознакомиться с инструкцией по вводу выражений.";
                        await client.SendTextMessageAsync(message.Chat.Id, welcomeMessage);

                        break;


                    case "/restart":
                        Process.GetCurrentProcess().Kill(); // stop bot process
                        Process.Start("path/to/bot/executable"); // start bot process again
                        var restartMessage = "бот перезапущен";
                        await client.SendTextMessageAsync(message.Chat.Id, restartMessage);
                        break;


                    case "/buildgraph":
                        if (!string.IsNullOrEmpty(message.Text))
                        {
                            var grafmessage = "График будет построен по вашей формуле. Пожалуйста, подождите...";
                            await client.SendTextMessageAsync(update.Message.Chat.Id, grafmessage);
                            string formula = message.Text;

                            // Отправляем сообщение с графиком
                            await client.SendTextMessageAsync(update.Message.Chat.Id, "График, построенный по вашей формуле:");

                            // Вызываем метод для построения графика
                            instance.InitializeGraph(formula); // Теперь мы используем instance

                            // Обновляем значение TextBoxFormula
                            instance.textBoxFormula.Text = formula;
                        }
                        else
                        {
                            var errorMessage = "Ошибка: не указана формула. Пожалуйста, введите формулу.";
                            await Error(client, new Exception(errorMessage), token);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                var errorMessage = "Ошибка: не указана формула. Пожалуйста, введите формулу.";
                await Error(client, new Exception(errorMessage), token);
            }

        }


        private static Task Error(ITelegramBotClient client, Exception exception, System.Threading.CancellationToken token)
        {
            Console.WriteLine($"Error: {exception.Message}");
            return Task.CompletedTask;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Получаем формулу из текстового поля или другого источника
            string userFormula = textBoxFormula.Text;

            InitializeGraph(userFormula);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }
    }


    
}




/*
 * if (update == null || update.Message == null)
            {
                var errorMessage = "Ошибка: не указана формула. Пожалуйста, введите формулу.";
        await Error(client, new Exception(errorMessage), token);
                await client.SendTextMessageAsync(message.Chat.Id, "Пожалуйста, введите новую формулу.");
    }
 * 
 * 
 * 
 * 
 * 
 * private void PlotGraph(string formula)
        {
            var plt = new ScottPlot.Plot(600, 400);

            // Создаем выражение на основе строки формулы
            Expression expression = new Expression(formula);
            Func<double, double?> function = x =>
            {
                var result = expression.Evaluate(new { x });

                // Проверяем, удалось ли выполнить вычисление, и возвращаем результат
                return result is double value ? (double?)value : null;
            };
            // Создаем делегат для использования с PlotFunction
            Func<double, double> function = x => Convert.ToDouble(expression.Evaluate(new { x }));

            // Строим график функции
            plt.PlotFunction(function);

            // Отображение графика в PictureBox на вашей форме (pictureBox1)
            plt.Render();

            // Убедитесь, что pictureBox1 есть на вашей форме
            pictureBox1.Image = plt.GetBitmap();
        }
        private Func<double, double> GetFunctionFromText(string formulaText)
        {
            try
            {
                Expression expression = new Expression(formulaText);
                Func<double, double> function = x => Convert.ToDouble(expression.Evaluate(new { x }));
                return function;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обработке формулы: {ex.Message}");
                return x => 0; // В случае ошибки возвращаем фиксированное значение
            }
        }*/
