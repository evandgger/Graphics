using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Example.AdditionalForms.AboutForm;
using Example.AdditionalForms.HelpForm;

namespace Example
{
    public partial class ExampleForm : Form
    {
        public ExampleForm()
        {
            InitializeComponent();
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var aboutForm = new AboutForm(); // створюмо нову форму

            aboutForm.Show(); // відтворюємо її
        }

        private void HelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var helpForm = new HelpForm(); // створюмо нову форму

            helpForm.Show(); // відтворюємо її
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var complexGraphic = Chart.Series[Constants.ComplexGraphic]; // дістаємо будь-який графік

            var pointsOfX = complexGraphic.Points.Select(item => item.XValue).ToList(); // з нього дістаємо усі значення по осі X

            if (pointsOfX.Any()) // if (pointsOfX.Count > 0) якщо є хоча б одна точка
            {
                try // Фрагмент коду, у якому можливі непередбачувані помилки. Краще виділити з try catch
                {
                    var directory = Path.GetDirectoryName(Constants.Filename); // беремо назву папки, куди додається файл

                    if (string.IsNullOrWhiteSpace(directory)) // якщо назву папки неможливо встановити, тобто шлях є хибним, повертаємо помилку
                    {
                        throw new FileNotFoundException(Constants.FileIsNotFoundMessage);
                    }

                    if (!Directory.Exists(directory)) // перевіряємо, чи існує така папка взагалі
                    {
                        Directory.CreateDirectory(directory); // якщо ні, створюємо її 
                    }

                    if (File.Exists(Constants.Filename)) // якщо вже існує файл з такою назвою
                    {
                        File.Delete(Constants.Filename); // видаляємо його
                    }

                    using (var fileStream = File.CreateText(Constants.Filename)) // створюємо новий текстовий файл для запису
                    {
                        foreach (var x in pointsOfX) // записуємо усі наші точки по осі X
                        {
                            fileStream.Write(x);

                            if (pointsOfX.IndexOf(x) != pointsOfX.Count - 1) // якщо точка не є останньою, додаємо пробіл щоб розділити їх між собою
                            {
                                fileStream.Write(Constants.FileSeparator);
                            }
                        }

                        MessageBox.Show(Constants.FileSuccessfullyCreatedMessage, Constants.SuccessCaption); // повідомляємо користувача, що операція успішна
                    }
                }
                catch (Exception exception) // якщо трапилася помилка, повідомляємо про неї
                {
                    MessageBox.Show(exception.Message, Constants.UnexpectedErrorCaption);
                }
            }
            else // якщо точок немає, повідомляємо користувача про це
            {
                MessageBox.Show(Constants.NoPointsForSaveMessage, Constants.InvalidInputCaption);
            }
        }

        private void RestoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(Constants.Filename)) // якщо вказаного файлу не існує
                {
                    throw new FileNotFoundException(Constants.FileIsNotFoundMessage); // повідомляємо про помилку
                }

                var fileLines = File.ReadAllLines(Constants.Filename).ToList(); // зчитуємо усі текстові рядки з файлу
                var pointsValues = fileLines.First().Split(Constants.FileSeparator).ToList(); // беремо перший рядок і розділяємо його через пробіл

                var points = pointsValues.Select(Convert.ToDouble).ToList(); // конвертуємо усі значення до типу double 

                if (!points.Any()) // якщо точок немає
                {
                    throw new InvalidOperationException(Constants.FileIsEmptyMessage); // повідомляємо, що файл пустий
                }

                RestoreInputs(points); // встановлюємо значення для полів вводу
                DrawGraphics(points); // малюємо графіки

                MessageBox.Show(Constants.FileSuccessfullyLoadedMessage, Constants.SuccessCaption); // повідомляємо, що операція успішна
            }
            catch (Exception exception) // якщо виникла непердбачувана помилка, повідомляємо про неї
            {
                MessageBox.Show(exception.Message, Constants.UnexpectedErrorCaption);
            }
        }

        private void RestoreInputs(List<double> points)
        {
            var orderedPoints = points.OrderBy(item => item).ToList(); // сортуємо точки, аби впевнитися у їх послідовності

            var a = orderedPoints.First(); 
            var b = orderedPoints.Last();
            var n = orderedPoints.Count; // дістаємо граничні значення і кількість точок

            InputA.Text = a.ToString();
            InputB.Text = b.ToString();
            InputN.Text = n.ToString(); // встановлюмо текстові значення для полів
        }

        private void GenerateButton_Click(object sender, EventArgs e)
        {
            var aText = InputA.Text;
            var bText = InputB.Text; 
            var nText = InputN.Text; // беремо текстові рядки зі значеннями

            var isAValid = double.TryParse(aText, out var a);
            var isBValid = double.TryParse(bText, out var b);
            var isNValid = int.TryParse(nText, out var n); // намагаємося зчитати їх у double та int
            // функція TryParse у будь-якого типу даних намагається конвертувати рядок. Якщо це немможливо, вона поверне false
            // якщо дані валідні, тобто коректні, TryParse записує їх у змінну, яка передається за допомогою "out var назва"

            if (isAValid && isBValid && isNValid) // якщо користувач ввів усі дані правильно
            {
                try // оскільки може виникнути будь-яка помилка при генерації, яка зламає нам програму
                // краще виділити її у try catch. У випадку помилки вивести користувачеві повідомлення 
                {
                    var pointsForX = GeneratePointsForX(a, b, n); // генеруємо значення точок на осі X

                    DrawGraphics(pointsForX); // малюємо графіки на основі створених точок
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message, Constants.UnexpectedErrorCaption);
                }
            }
            else
            {
                var message = GetMessageForInvalidInput(isAValid, isBValid, isNValid);

                MessageBox.Show(message, Constants.InvalidInputCaption);
            }
        }

        private List<double> GeneratePointsForX(double a, double b, int n)
        {
            if (a > b)
            {
                Swap(a, b); // якщо користувач помилився і задав неправильні значення
            }

            var range=  b - a; // Рахуємо загальну відстань 
            var step = range / (n - 1); // Щоб права границя також увійшла до списку точок, крок необхідно трохи збільшити

            var pointOfX = new List<double>(); // List це тип даних подібний до масиву. У мові C# значно зручніше користуватися саме List  

            for (var i = 0; i < n; i++)
            {
                var x = a + step * i;
                var roundedX = Math.Round(x, 2); // У результаті математичних операцій можуть зв'явитися погрішності, або періоди, наприклад 1/3 = 0.333333333. 
                // щоб уникнути великої кількості знаків після коми, краще округлити значення до 2 знаків

                pointOfX.Add(roundedX);
            }

            return pointOfX;
        }

        private void DrawGraphics(List<double> pointsForX)
        {
            var complexGraphic = Chart.Series[Constants.ComplexGraphic];
            var simpleGraphic = Chart.Series[Constants.SimpleGraphic];
            // назви графіків зручно помістити в константи

            complexGraphic.Points.Clear();
            simpleGraphic.Points.Clear(); // очистили точки з попередніх розрахунків

            foreach (var x in pointsForX)
            {
                complexGraphic.Points.AddXY(x, ComplexFunction(x));
                simpleGraphic.Points.AddXY(x, SimpleFunction(x)); // додали нові точки, які рахуються відповідними функціями
            }
        }

        private double ComplexFunction(double x)
        {
            var value = Math.Cos(x) + Math.Sin(x);

            return Math.Round(value, 2);
        }

        private double SimpleFunction(double x)
        {
            var value = x;

            return Math.Round(value, 2);
        }

        private void Swap(object a, object b)
        {
            var temp = a;
            a = b;
            b = temp;
        }

        private string GetMessageForInvalidInput(bool isAValid, bool isBValid, bool isNValid)
        {
            var result = string.Empty;

            if (!isAValid)
            {
                result += $"A value is not valid\n";
            }

            if (!isBValid)
            {
                result += $"B value is not valid\n";
            }

            if (!isNValid)
            {
                result += $"N value is not valid\n";
            }

            return result;
        }
    }
}