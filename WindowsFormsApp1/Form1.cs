using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telegram.Bot;
using Telegram.Bot.Types;
using ZedGraph;
using System.IO;
using org.mariuszgromada.math.mxparser;
using System.Drawing;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        string expectingForFormula;
        private TelegramBotClient botClient;


        public Form1()
        {
            InitializeComponent();
        }
        

        private void button1_Click(object sender, EventArgs e)
        {
            botClient = new TelegramBotClient("6820445732:AAHzvCAkkuFVYO7siXIJR5nppkNDWdUV-j4");
            botClient.StartReceiving(UpdateBot, Error);
        }
        private static async Task Error(ITelegramBotClient client, Exception exception, System.Threading.CancellationToken token)
        {

        }


        private async Task UpdateBot(ITelegramBotClient client, Update update, System.Threading.CancellationToken token)
        {
            switch (update.Message.Text)
            {
                case "/start":
                    var welcomeMessage = "Вас приветствует наш бот!\n" +
                        " Перед использованием нашего тг-бота рекомендуется воспользоваться инструкцией по вводу выражений по команде /help в меню.";
                    client.SendTextMessageAsync(update.Message.Chat.Id, welcomeMessage).GetAwaiter().GetResult();
                    break;
                case "/help":
                    var helpMessage = " Здесь приведены готовые шаблоны выражений, которые могут меняться с их аргументами.\n" +
                        "\n" +
                        " Также рекомендуется использовать пробелы между операциями, переменными и константами.\n" +
                        "\n" +
                        "-  Возведение в степень:   x^n ;\n" +
                        "\n" +
                        "-  Число ПИ:   pi\n" +
                        "\n" +
                        "-  sin(x)    cos(x)   tan(x) ;\n" +
                        "\n" +
                        "-   COT(x)   в этом боте записывается как:    1 / tan(x)    ;\n" +
                        "\n" +
                        "-  asin(x)   acos(x)   atan(x) ;\n" +
                        "\n" +
                        "-   ACOT(x)   в этом боте записывается как:     pi/2 - atan(x)     ;\n" +
                        "\n" +
                        "-  sinh(x)   cosh(x)  - ГИПЕРБОЛИЧЕСКИЕ синус и косинус;\n" +
                        "\n" +
                        "-   exp(x) - в скобках указывается степень, в которой будет стоять ЭКСПОНЕНТА;\n" +
                        "\n" +
                        "-   log10(x) - десятичный логарифм;\n" +
                        "\n" +
                        "-   ln(x) или log(x) - два варианта записи натуральных логарифмов;\n" +
                        "\n" +
                        "-   log_b(x) - логарифм с произвольным основанием.";
                    client.SendTextMessageAsync(update.Message.Chat.Id, helpMessage).GetAwaiter().GetResult();
                    break;
                default:
                    expectingForFormula = update.Message.Text;
                    if ((update.Message.Text != "/start") || (update.Message.Text != "/help"))
                    {
                        if (string.IsNullOrEmpty(update.Message.Text))
                        {
                            expectingForFormula = update.Message.Text;
                            var elseMessage = "Бот не поддерживает данный вид выражения. Попробуйте снова...";
                            await client.SendTextMessageAsync(update.Message.Chat.Id, elseMessage);
                        }
                        else
                        {
                            expectingForFormula = update.Message.Text;
                            var buildgraph = "Секундочку, сейчас построю график...";
                            client.SendTextMessageAsync(update.Message.Chat.Id, buildgraph).GetAwaiter().GetResult();

                            try
                            {
                                Func<double, double> func;
                                ZedGraphControl zedGraphControl = new ZedGraphControl
                                {
                                    Dock = DockStyle.Fill,
                                    Width = 800, 
                                    Height = 600
                                };

                                GraphPane graphPane = zedGraphControl.GraphPane;
                                graphPane.Title.Text = "График функции";
                                graphPane.XAxis.Title.Text = "X";
                                graphPane.YAxis.Title.Text = "Y";

                                PointPairList pointPairList = new PointPairList();
                                double minX = -10;
                                double maxX = 10;
                                double step = 0.0001;

                                func = Task.Run(() => ParseFormulaAsync(expectingForFormula)).GetAwaiter().GetResult();
                                for (double x = minX; x <= maxX; x += step)
                                {
                                    double y = func(x);
                                    pointPairList.Add(x, y);
                                }

                                LineItem curve = graphPane.AddCurve("Функция", pointPairList, System.Drawing.Color.Red, SymbolType.None);

                                zedGraphControl.AxisChange();
                                zedGraphControl.Invalidate();

                                var filePath = $"graph_{update.Message.Chat.Id}.png";
                                int desiredDpi = 300;

                                Bitmap bitmap = new Bitmap(zedGraphControl.Width, zedGraphControl.Height);
                                zedGraphControl.DrawToBitmap(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));

                                bitmap.SetResolution(desiredDpi, desiredDpi);
                                bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);

                                using (var stream = new FileStream(filePath, FileMode.Open))
                                {
                                    botClient.SendPhotoAsync(update.Message.Chat.Id, InputFile.FromStream(stream)).GetAwaiter().GetResult();
                                }

                                System.IO.File.Delete(filePath);
                                var endbuildgraph = "Готово!";
                                client.SendTextMessageAsync(update.Message.Chat.Id, endbuildgraph).GetAwaiter().GetResult();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error parsing formula: {ex.Message}");
                            }
                        }
                        expectingForFormula = null;
                    }
            break;
            }
        }
        private Func<double, double> ParseFormulaAsync(string formulaText)
        {
            Function func = new Function($"f(x) = {formulaText}");
            return (double x) => func.calculate(x);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            botClient = new TelegramBotClient("6820445732:AAHzvCAkkuFVYO7siXIJR5nppkNDWdUV-j4");


            //тут запускаем бота фактически и задаем обработчики
            //когда возникает новое сообщение оно обрабатывается тут, вызываются обработчики и в них передаются аргументы с сообщением пользователем и т.д.
            botClient.StartReceiving(UpdateBot, Error);
        }

        private void zedGraphControl1_Load(object sender, EventArgs e)
        {

        }
    }
}



/*
        private async Task<Func<double, double>> ParseFormulaSinAsync(string formulaText)
        {
            return await Task.Run(() =>
            {
                ParameterExpression x = Expression.Parameter(typeof(double), "x");
                string modifiedFormula = formulaText.Replace("sin", "Math.Sin");

                LambdaExpression e = DynamicExpressionParser.ParseLambda(new ParameterExpression[] { x }, null, modifiedFormula);
                return (Func<double, double>)e.Compile();
            });
        }*/

/*private async Task<Func<double, double>> ParseFormulaAsync(string formulaText)
{
    return await Task.Run(() =>
    {
        org.mariuszgromada.math.mxparser.Expression expression = new org.mariuszgromada.math.mxparser.Expression(formulaText, new Argument("x"));
        if (expression.checkSyntax())
        {
            Func<double, double> func = x => expression.calculate();
            return func;
        }
        else
        {
            Console.WriteLine($"Error in formula syntax: {expression.getErrorMessage()}");
            return null;
        }
    });
}*/



/*
Func<double, double> func;
ZedGraphControl zedGraphControl = new ZedGraphControl
{
    Dock = DockStyle.Fill,
    Width = 800, // Установите желаемую ширину графика
    Height = 600 // Установите желаемую высоту графика
};

// Создание графика
GraphPane graphPane = zedGraphControl.GraphPane;
graphPane.Title.Text = "График функции";
graphPane.XAxis.Title.Text = "X";
graphPane.YAxis.Title.Text = "Y";


PointPairList pointPairList = new PointPairList();
double minX = -10;
double maxX = 10;
double step = 0.0001;


//BUILD2
var buildgraph = "build2";
//await client.SendTextMessageAsync(update.Message.Chat.Id, buildgraph);
client.SendTextMessageAsync(update.Message.Chat.Id, buildgraph).GetAwaiter().GetResult();
//

//BUILD3
var ifbuildgrapsin = "build3";
client.SendTextMessageAsync(update.Message.Chat.Id, ifbuildgrapsin).GetAwaiter().GetResult();

//func = await Task.Run(() => ParseFormulaSinAsync(expectingForFormula));
func = Task.Run(() => ParseFormulaAsync(expectingForFormula)).GetAwaiter().GetResult();
for (double x = minX; x <= maxX; x += step)
{
    double y = func(x);
    pointPairList.Add(x, y);
}

// Добавление кривой на график
LineItem curve = graphPane.AddCurve("Функция", pointPairList, System.Drawing.Color.Red, SymbolType.None);


// Обновление графика
zedGraphControl.AxisChange();
zedGraphControl.Invalidate();

//BUILD4
var buildgrapsin = "build4";
client.SendTextMessageAsync(update.Message.Chat.Id, buildgrapsin).GetAwaiter().GetResult();

var filePath = $"test_graph_{update.Message.Chat.Id}.png";
//zedGraphControl.SaveAs(filePath);
int desiredDpi = 300; // Желаемое разрешение в точках на дюйм

Bitmap bitmap = new Bitmap(zedGraphControl.Width, zedGraphControl.Height);
zedGraphControl.DrawToBitmap(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));

bitmap.SetResolution(desiredDpi, desiredDpi);
bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);

//zedGraphControl.MasterPane.GetImage().Save(filePath);

//BUILD5
var buildpng = "png создан5555";
client.SendTextMessageAsync(update.Message.Chat.Id, buildpng).GetAwaiter().GetResult();

using (var stream = new FileStream(filePath, FileMode.Open))
{
    botClient.SendPhotoAsync(update.Message.Chat.Id, InputFile.FromStream(stream)).GetAwaiter().GetResult();
}

System.IO.File.Delete(filePath);


var build2Message = "BUILD РАБОТАЕТ";
client.SendTextMessageAsync(update.Message.Chat.Id, build2Message).GetAwaiter().GetResult();


                            }*/