﻿using System;
using System.Globalization;
using NUnit.Framework;
using FluentAssertions;


namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .ExcludeType<int>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .SerializeType<string>(x => x.ToUpper())
                //3. Для числовых типов указать культуру
                .SetCulture<int>(CultureInfo.CurrentCulture)
                .ExcludeProperty(typeof(Person).GetProperty("Name")).
                //4. Настроить сериализацию конкретного свойства
                SerializeProperty("Age").SetSerialization(x => x + " лет")
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .SelectString("Name").Trim(10);
                //6. Исключить из сериализации конкретного свойства
                //.ExcludeProperty(p => p.name);
            
            string s1 = printer.PrintToString(person);
            
            Console.WriteLine(s1);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием
        }

        [Test]
        public void Serialization_NoCircularRecursion_SameObjectsOneLevel()
        {
            var person1 = new Person3 {Id = new Guid(), Name = "Alex"};
            var person2 = new Person3 {Id = new Guid(), Name = "Joe", };
            person1.Friend1 = person2;
            person1.Friend2 = person2;
            var printer = ObjectPrinter.For<Person3>();
            string s1 = printer.PrintToString(person1);
            string result = "Person3" + Environment.NewLine +
                            "\tId = 00000000-0000-0000-0000-000000000000" + Environment.NewLine +
                            "\tName = Alex" + Environment.NewLine +
                            "\tFriend1 = Person3" + Environment.NewLine +
                            "\t\tId = 00000000-0000-0000-0000-000000000000" + Environment.NewLine +
                            "\t\tName = Joe" + Environment.NewLine +
                            "\t\tFriend1 = null" + Environment.NewLine +
                            "\t\tFriend2 = null" + Environment.NewLine +
                            "\tFriend2 = Person3" + Environment.NewLine +
                            "\t\tId = 00000000-0000-0000-0000-000000000000" + Environment.NewLine +
                            "\t\tName = Joe" + Environment.NewLine +
                            "\t\tFriend1 = null" + Environment.NewLine +
                            "\t\tFriend2 = null" + Environment.NewLine;

            s1.Should().Be(result);
            //Console.WriteLine(s1);
        }
        
        [Test]
        public void Serialization_NoCircularRecursion()
        {
            var person1 = new Person2 {Id = new Guid(), Name = "Alex"};
            var person2 = new Person2 {Id = new Guid(), Friend = person1, Name = "Joe", };
            person1.Friend = person2;
            var printer = ObjectPrinter.For<Person2>();
            string s1 = printer.PrintToString(person1);
            string result = "Person2" + Environment.NewLine +
                            "\tId = 00000000-0000-0000-0000-000000000000" + Environment.NewLine +
                            "\tName = Alex" + Environment.NewLine +
                            "\tFriend = Person2" + Environment.NewLine +
                            "\t\tId = 00000000-0000-0000-0000-000000000000" + Environment.NewLine +
                            "\t\tName = Joe" + Environment.NewLine +
                            "\t\tFriend = (cycle reference)" + Environment.NewLine;

            s1.Should().Be(result);
            //Console.WriteLine(s1);
        }
        
        [Test]
        public void Serialization_CropInt_ThrowException()
        {
            var person = new Person { Name = "Alex Ivanov", Age = 19, Height = 70.5, Id = new Guid()};

            var printer = ObjectPrinter.For<Person>()
                .SelectString("Age").Trim(4);
            
            Action act = () => printer.PrintToString(person);

            act.Should().Throw<ArgumentException>();
        }
        
        
        [Test]
        public void Serialization_CropShortString()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 70.5, Id = new Guid()};

            var printer = ObjectPrinter.For<Person>()
                .SelectString("Name").Trim(10);
            
            Action act = () => printer.PrintToString(person);

            act.Should().Throw<ArgumentException>();
        }
        
        [Test]
        public void Serialization_CropString()
        {
            var person = new Person { Name = "Alex Ivanov", Age = 19, Height = 70.5, Id = new Guid()};

            var printer = ObjectPrinter.For<Person>()
                .SelectString("Name").Trim(4);
            
            string s1 = printer.PrintToString(person);
            string result = "Person" + Environment.NewLine +
                            "\tId = 00000000-0000-0000-0000-000000000000" + Environment.NewLine +
                            "\tName = Alex" + Environment.NewLine +
                            "\tHeight = 70.5" + Environment.NewLine +
                            "\tAge = 19" + Environment.NewLine;
            s1.Should().Be(result);
            //Console.WriteLine(s1);
        }
        
        [Test]
        public void Serialization_AgeAndHeight()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 70.5, Id = new Guid()};

            var printer = ObjectPrinter.For<Person>()
                .SerializeProperty("Age").SetSerialization(x => x + " years old")
                .SerializeProperty("Height").SetSerialization(x => x + " kg");
            
            string s1 = printer.PrintToString(person);
            string result = "Person" + Environment.NewLine +
                            "\tName = Alex" + Environment.NewLine +
                            "\tId = 00000000-0000-0000-0000-000000000000" + Environment.NewLine +
                            "\tHeight = 70.5 kg" + Environment.NewLine +
                            "\tAge = 19 years old" + Environment.NewLine;
            s1.Should().Be(result);
            Console.WriteLine(s1);
        }
        
        [Test]
        public void Serialization_Age()
        {
            var person = new Person { Name = "Alex", Id = new Guid(), Age = 19, Height = 70.5};

            var printer = ObjectPrinter.For<Person>()
                .SerializeProperty("Age").SetSerialization(x => x + " лет");
            
            string s1 = printer.PrintToString(person);
            string result = "Person" + Environment.NewLine +
                            "\tName = Alex" + Environment.NewLine +
                            "\tId = 00000000-0000-0000-0000-000000000000" + Environment.NewLine +
                            "\tHeight = 70.5" + Environment.NewLine +
                            "\tAge = 19 лет" + Environment.NewLine;
            s1.Should().Be(result);
            Console.WriteLine(s1);
        }
        
        [Test]
        public void ExcludePropertyNameAndExcludeTypeInt()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 70.5, Id = new Guid()};

            var printer = ObjectPrinter.For<Person>()
                .ExcludeProperty(typeof(Person).GetProperty("Name"))
                .ExcludeType<int>();
            
            string s1 = printer.PrintToString(person);
            string result = "Person" + Environment.NewLine +
                            "\tId = 00000000-0000-0000-0000-000000000000" + Environment.NewLine +
                            "\tHeight = 70.5" + Environment.NewLine;
            s1.Should().Be(result);
            //Console.WriteLine(s1);
        }
        
        [Test]
        public void ExcludeProperty_Name()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 70.5, Id = new Guid()};

            var printer = ObjectPrinter.For<Person>()
                .ExcludeProperty(typeof(Person).GetProperty("Name"));
            
            string s1 = printer.PrintToString(person);
            string result = "Person" + Environment.NewLine +
                            "\tId = 00000000-0000-0000-0000-000000000000" + Environment.NewLine +
                            "\tHeight = 70.5" + Environment.NewLine +
                            "\tAge = 19" + Environment.NewLine;
            s1.Should().Be(result);
            //Console.WriteLine(s1);
        }
        
        [Test]
        public void SetCulture_Double()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 70.5, Id = new Guid()};

            var printer = ObjectPrinter.For<Person>()
                .SetCulture<double>(CultureInfo.GetCultureInfo("en-GB"));
            
            string s1 = printer.PrintToString(person);
            string result = "Person" + Environment.NewLine +
                            "\tId = 00000000-0000-0000-0000-000000000000" + Environment.NewLine +
                            "\tName = Alex" + Environment.NewLine +
                            "\tHeight = 70.5" + Environment.NewLine +
                            "\tAge = 19" + Environment.NewLine;
            s1.Should().Be(result);
            
            
            //Console.WriteLine(s1);
        }

        [Test]
        public void ExcludeType_Int()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 70, Id = new Guid()};

            var printer = ObjectPrinter.For<Person>()
                .ExcludeType<int>();
            
            string s1 = printer.PrintToString(person);
            string result = "Person" + Environment.NewLine +
                            "\tName = Alex" + Environment.NewLine +
                            "\tId = 00000000-0000-0000-0000-000000000000" + Environment.NewLine +
                            "\tHeight = 70" + Environment.NewLine;
            s1.Should().Be(result);
        }
        
        [Test]
        public void ExcludeType_String()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 70, Id = new Guid()};

            var printer = ObjectPrinter.For<Person>()
                .ExcludeType<string>();
            
            string s1 = printer.PrintToString(person);
            string result = "Person" + Environment.NewLine +
                            "\tId = 00000000-0000-0000-0000-000000000000" + Environment.NewLine +
                            "\tHeight = 70" + Environment.NewLine +
                            "\tAge = 19" + Environment.NewLine;
            s1.Should().Be(result);
            
            //Console.WriteLine(s1);
        }
        
        [Test]
        public void SerializeType_String()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 70, Id = new Guid()};

            var printer = ObjectPrinter.For<Person>()
                .SerializeType<string>(x => x.ToUpper());
            
            string s1 = printer.PrintToString(person);
            string result = "Person" + Environment.NewLine +
                            "\tId = 00000000-0000-0000-0000-000000000000" + Environment.NewLine +
                            "\tName = ALEX" + Environment.NewLine +
                            "\tHeight = 70" + Environment.NewLine +
                            "\tAge = 19" + Environment.NewLine;
            s1.Should().Be(result);
            
            //Console.WriteLine(s1);
        }
        [Test]
        public void SerializeType_Int()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 70, Id = new Guid()};

            var printer = ObjectPrinter.For<Person>()
                .SerializeType<int>(x => x + " лет");
            
            string s1 = printer.PrintToString(person);
            string result = "Person" + Environment.NewLine +
                            "\tId = 00000000-0000-0000-0000-000000000000" + Environment.NewLine +
                            "\tName = Alex" + Environment.NewLine +
                            "\tHeight = 70" + Environment.NewLine +
                            "\tAge = 19 лет" + Environment.NewLine;
            s1.Should().Be(result);
            
            Console.WriteLine(s1);
        }
        
        [Test]
        public void SerializeType_Double()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 70, Id = new Guid()};

            var printer = ObjectPrinter.For<Person>()
                .SerializeType<double>(x => x + " кг");
            
            string s1 = printer.PrintToString(person);
            string result = "Person" + Environment.NewLine +
                            "\tId = 00000000-0000-0000-0000-000000000000" + Environment.NewLine +
                            "\tName = Alex" + Environment.NewLine +
                            "\tHeight = 70 кг" + Environment.NewLine +
                            "\tAge = 19" + Environment.NewLine;
            s1.Should().Be(result);
            
            //Console.WriteLine(s1);
        }
        
        [Test]
        public void SerializeType_DoubleAndInt()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 70, Id = new Guid()};

            var printer = ObjectPrinter.For<Person>()
                .SerializeType<double>(x => x + " кг")
                .SerializeType<int>(x => x + " лет");
            
            string s1 = printer.PrintToString(person);
            string result = "Person" + Environment.NewLine +
                            "\tId = 00000000-0000-0000-0000-000000000000" + Environment.NewLine +
                            "\tName = Alex" + Environment.NewLine +
                            "\tHeight = 70 кг" + Environment.NewLine +
                            "\tAge = 19 лет" + Environment.NewLine;
            s1.Should().Be(result);
            
            //Console.WriteLine(s1);
        }
        
        [Test]
        public void ExcludeType_Double()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 70, Id = new Guid()};

            var printer = ObjectPrinter.For<Person>()
                .ExcludeType<double>();
            
            string s1 = printer.PrintToString(person);
            string result = "Person" + Environment.NewLine +
                            "\tName = Alex" + Environment.NewLine +
                            "\tId = 00000000-0000-0000-0000-000000000000" + Environment.NewLine +
                            "\tAge = 19" + Environment.NewLine;
            s1.Should().Be(result);
            
            //Console.WriteLine(s1);
        }
    }
}