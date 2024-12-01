using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace QuizApp
{
    class Program
    {
        static string DataFile = "data.json";

        static Dictionary<string, User> Users = new();
        static List<Quiz> Quizzes = new();
        static User CurrentUser;

        static void Main(string[] args)
        {
            LoadData();

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Добро пожаловать в Викторину!");
                Console.WriteLine("1. Вход");
                Console.WriteLine("2. Регистрация");
                Console.WriteLine("3. Выход");

                switch (Console.ReadLine())
                {
                    case "1": Login(); break;
                    case "2": Register(); break;
                    case "3": SaveData(); return;
                    default: Console.WriteLine("Неверный выбор!"); break;
                }
            }
        }

        static void Login()
        {
            Console.Clear();
            Console.Write("Введите логин: ");
            string login = Console.ReadLine();
            Console.Write("Введите пароль: ");
            string password = Console.ReadLine();

            if (Users.ContainsKey(login) && Users[login].Password == password)
            {
                CurrentUser = Users[login];
                UserMenu();
            }
            else
            {
                Console.WriteLine("Неверный логин или пароль!");
                Console.ReadKey();
            }
        }

        static void Register()
        {
            Console.Clear();
            Console.Write("Придумайте логин: ");
            string login = Console.ReadLine();
            if (Users.ContainsKey(login))
            {
                Console.WriteLine("Этот логин уже занят!");
                Console.ReadKey();
                return;
            }

            Console.Write("Придумайте пароль: ");
            string password = Console.ReadLine();
            Console.Write("Введите дату рождения (дд.мм.гггг): ");
            DateTime.TryParse(Console.ReadLine(), out DateTime birthDate);

            Users[login] = new User { Login = login, Password = password, BirthDate = birthDate };
            Console.WriteLine("Регистрация успешна!");
            Console.ReadKey();
        }

        static void UserMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Добро пожаловать, {CurrentUser.Login}!");
                Console.WriteLine("1. Начать новую викторину");
                Console.WriteLine("2. Посмотреть результаты");
                Console.WriteLine("3. Посмотреть Топ-20 игроков");
                Console.WriteLine("4. Настройки");
                Console.WriteLine("5. Выйти");

                switch (Console.ReadLine())
                {
                    case "1": StartQuiz(); break;
                    case "2": ViewResults(); break;
                    case "3": ViewLeaderboard(); break;
                    case "4": Settings(); break;
                    case "5": return;
                    default: Console.WriteLine("Неверный выбор!"); break;
                }
            }
        }

        static void StartQuiz()
        {
            Console.Clear();
            Console.WriteLine("Выберите категорию викторины:");
            for (int i = 0; i < Quizzes.Count; i++)
                Console.WriteLine($"{i + 1}. {Quizzes[i].Category}");

            if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= Quizzes.Count)
            {
                Quiz selectedQuiz = Quizzes[choice - 1];

                int score = 0;

                foreach (var question in selectedQuiz.Questions)
                {
                    Console.Clear();
                    Console.WriteLine(question.Text);
                    for (int i = 0; i < question.Options.Count; i++)
                        Console.WriteLine($"{i + 1}. {question.Options[i]}");

                    Console.Write("Ваш выбор: ");
                    int.TryParse(Console.ReadLine(), out int answer);

                    if (question.CorrectAnswers.Contains(answer - 1))
                        score++;
                }

                CurrentUser.Results.Add(new QuizResult
                {
                    QuizCategory = selectedQuiz.Category,
                    Score = score
                });

                Console.WriteLine($"Вы завершили викторину с результатом: {score}/{selectedQuiz.Questions.Count}");

                // Дополнительная информация:
                if (selectedQuiz.Category == "Угадай мощную видеокарту NVIDIA")
                    Console.WriteLine($"Ваша общая производительность: {score * 10} FPS в Forza Horizon 5");
                else if (selectedQuiz.Category == "Угадай мощную BMW")
                    Console.WriteLine($"Общая мощность: {score * 50} л.с.");

                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("Неверный выбор!");
                Console.ReadKey();
            }
        }

        static void ViewResults()
        {
            Console.Clear();
            Console.WriteLine("Ваши результаты:");
            foreach (var result in CurrentUser.Results)
                Console.WriteLine($"Категория: {result.QuizCategory}, Результат: {result.Score}");

            Console.ReadKey();
        }

        static void ViewLeaderboard()
        {
            Console.Clear();
            Console.WriteLine("Выберите категорию для просмотра Топ-20:");
            for (int i = 0; i < Quizzes.Count; i++)
                Console.WriteLine($"{i + 1}. {Quizzes[i].Category}");

            if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= Quizzes.Count)
            {
                string category = Quizzes[choice - 1].Category;

                var leaderboard = Users.Values
                    .SelectMany(u => u.Results
                        .Where(r => r.QuizCategory == category)
                        .Select(r => new { u.Login, r.Score }))
                    .OrderByDescending(r => r.Score)
                    .Take(20)
                    .ToList();

                Console.Clear();
                Console.WriteLine($"Топ-20 игроков в категории \"{category}\":");
                for (int i = 0; i < leaderboard.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {leaderboard[i].Login} - {leaderboard[i].Score} очков");
                }

                if (!leaderboard.Any())
                    Console.WriteLine("Пока нет результатов для этой категории.");

                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("Неверный выбор!");
                Console.ReadKey();
            }
        }

        static void Settings()
        {
            Console.Clear();
            Console.WriteLine("1. Изменить пароль");
            Console.WriteLine("2. Изменить дату рождения");

            switch (Console.ReadLine())
            {
                case "1":
                    Console.Write("Введите новый пароль: ");
                    CurrentUser.Password = Console.ReadLine();
                    Console.WriteLine("Пароль изменён.");
                    break;
                case "2":
                    Console.Write("Введите новую дату рождения (дд.мм.гггг): ");
                    DateTime.TryParse(Console.ReadLine(), out DateTime newDate);
                    CurrentUser.BirthDate = newDate;
                    Console.WriteLine("Дата рождения изменена.");
                    break;
                default: Console.WriteLine("Неверный выбор!"); break;
            }
            Console.ReadKey();
        }

        static void LoadData()
        {
            if (File.Exists(DataFile))
            {
                string json = File.ReadAllText(DataFile);
                var data = JsonSerializer.Deserialize<SaveDataModel>(json);
                if (data != null)
                {
                    Users = data.Users;
                    Quizzes = data.Quizzes;
                }
            }
            else
            {
                InitializeSampleData();
            }
        }

        static void SaveData()
        {
            var data = new SaveDataModel
            {
                Users = Users,
                Quizzes = Quizzes
            };

            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(DataFile, json);
        }

        static void InitializeSampleData()
        {
            // Викторина на тему "Какая видеокарта NVIDIA мощнее"
            Quizzes.Add(new Quiz
            {
                Category = "Какая видеокарта NVIDIA мощнее",
                Questions = new List<Question>
        {
            new Question
            {
                Text = "Какая видеокарта мощнее?",
                Options = new List<string> { "RTX 3060", "RTX 3070", "RTX 3050" },
                CorrectAnswers = new List<int> { 1 }
            },
            new Question
            {
                Text = "Какая видеокарта мощнее?",
                Options = new List<string> { "RTX 3080", "RTX 3070", "RTX 3060 Ti" },
                CorrectAnswers = new List<int> { 0 }
            },
            new Question
            {
                Text = "Какая видеокарта мощнее?",
                Options = new List<string> { "GTX 1080ti", "GTX 1660", "RTX 2060" },
                CorrectAnswers = new List<int> { 0 }
            },
            new Question
            {
                Text = "Какая видеокарта мощнее?",
                Options = new List<string> { "GT 720", "GTX 660", "GT 750M" },
                CorrectAnswers = new List<int> { 1 }
            }
        }
            });

            // Викторина на тему "Какая BMW мощнее"
            Quizzes.Add(new Quiz
            {
                Category = "Угадай какая BMW мощнее",
                Questions = new List<Question>
        {
            new Question
            {
                Text = "Какая BMW мощнее?",
                Options = new List<string> { "BMW M3", "BMW M5", "BMW 320i" },
                CorrectAnswers = new List<int> { 1 }
            },
            new Question
            {
                Text = "Какая BMW мощнее?",
                Options = new List<string> { "BMW X6", "BMW X5", "BMW X3" },
                CorrectAnswers = new List<int> { 0 }
            },
            new Question
            {
                Text = "Какая BMW мощнее?",
                Options = new List<string> { "BMW M3 F80", "BMW M3 E34", "BMW M3 G80 COMPETITION" },
                CorrectAnswers = new List<int> { 2 }
            },
            new Question
            {
                Text = "Какая BMW мощнее?",
                Options = new List<string> { "BMW f30 320", "BMW f30 320d", "BMW f30 318i" },
                CorrectAnswers = new List<int> { 0 }
            }
        }
            });

            // Викторина на тему "География"
            Quizzes.Add(new Quiz
            {
                Category = "География",
                Questions = new List<Question>
        {
            new Question
            {
                Text = "Какая страна славиться пивом?",
                Options = new List<string> { "Германия", "Дания" , "Филиппины" },
                CorrectAnswers = new List<int> { 0 }
            },
            new Question
            {
                Text = "Какая река самая длинная в мире?",
                Options = new List<string> { "Амазонка", "Нил", "Миссисипи" },
                CorrectAnswers = new List<int> { 1 }
            },
            new Question
            {
                Text = "Какой океан самый глубокий?",
                Options = new List<string> { "Тихий океан", "Атлантический океан", "Индийский океан" },
                CorrectAnswers = new List<int> { 0 }
            },
            new Question
            {
                Text = "Какое государство расположено на двух континентах?",
                Options = new List<string> { "Казахстан", "Турция", "Египет" },
                CorrectAnswers = new List<int> { 1 }
            }
        }
            });

            // Викторина на тему "История"
            Quizzes.Add(new Quiz
            {
                Category = "История",
                Questions = new List<Question>
        {
            new Question
            {
                Text = "Кто был первым президентом США?",
                Options = new List<string> { "Авраам Линкольн", "Джордж Вашингтон", "Томас Джефферсон" },
                CorrectAnswers = new List<int> { 1 }
            },
            new Question
            {
                Text = "Когда началась Вторая мировая война?",
                Options = new List<string> { "1939", "1941", "1914" },
                CorrectAnswers = new List<int> { 0 }
            },
            new Question
            {
                Text = "Кто был королем Англии в 1066 году?",
                Options = new List<string> { "Вильгельм Завоеватель", "Генрих VIII", "Ричард Львиное Сердце" },
                CorrectAnswers = new List<int> { 0 }
            },
            new Question
            {
                Text = "Какая страна первая поставила человека на Луну?",
                Options = new List<string> { "США", "СССР", "Китай" },
                CorrectAnswers = new List<int> { 0 }
            }
        }
            });

            // Викторина на тему "Смешанные вопросы"
            Quizzes.Add(new Quiz
            {
                Category = "Смешанная викторина",
                Questions = new List<Question>
        {
            new Question
            {
                Text = "Какая видеокарта мощнее?",
                Options = new List<string> { "RTX 3060", "RTX 3070", "RTX 3050" },
                CorrectAnswers = new List<int> { 1 }
            },
            new Question
            {
                Text = "Какая BMW мощнее?",
                Options = new List<string> { "BMW X6", "BMW X5", "BMW X3" },
                CorrectAnswers = new List<int> { 0 }
            },
            new Question
            {
                Text = "Какая река самая длинная в мире?",
                Options = new List<string> { "Амазонка", "Нил", "Миссисипи" },
                CorrectAnswers = new List<int> { 1 }
            },
            new Question
            {
                Text = "Когда началась Вторая мировая война?",
                Options = new List<string> { "1939", "1941", "1914" },
                CorrectAnswers = new List<int> { 0 }
            },
            new Question
            {
                Text = "Какой океан самый глубокий?",
                Options = new List<string> { "Тихий океан", "Атлантический океан", "Индийский океан" },
                CorrectAnswers = new List<int> { 0 }
            }
        }
            });
        }

    }

    class User
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public DateTime BirthDate { get; set; }
        public List<QuizResult> Results { get; set; } = new();
    }

    class Question
    {
        public string Text { get; set; }
        public List<string> Options { get; set; } = new();
        public List<int> CorrectAnswers { get; set; } = new();
    }

    class Quiz
    {
        public string Category { get; set; }
        public List<Question> Questions { get; set; } = new();
    }

    class QuizResult
    {
        public string QuizCategory { get; set; }
        public int Score { get; set; }
    }

    class SaveDataModel
    {
        public Dictionary<string, User> Users { get; set; } = new();
        public List<Quiz> Quizzes { get; set; } = new();
    }
}
