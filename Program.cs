using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

class Program
{
    public static async Task Main()
    {
        // ОТКРЫТИЕ ФАЙЛА
        StreamReader file = new StreamReader("in.txt");


        string fileLine = file.ReadLine(); // Количество поставщиков и потребителей
        string[] fileValues = fileLine.Split(' ');

        int providerAmount = int.Parse(fileValues[0]); // Кол-во поставщиков
        int consumerAmount = int.Parse(fileValues[1]); // Кол-во потребителей


        fileLine = file.ReadLine(); // Запасы поставщиков
        fileValues = fileLine.Split(' ');

        List<int> providerReserve = new List<int>();

        foreach (string value in fileValues)
            providerReserve.Add(int.Parse(value));

        
        fileLine = file.ReadLine(); // Потребности потребителей
        fileValues = fileLine.Split(' ');

        List<int> consumerNeed = new List<int>();

        foreach (string value in fileValues)
            consumerNeed.Add(int.Parse(value));

        
        List<List<int>> priceTable = new List<List<int>>(); // Стоимость грузоперевозок (таблица цен)

        for (int i = 0; i < providerAmount; i++)
        {
            fileLine = file.ReadLine(); // Одна из N строк таблицы цен
            fileValues = fileLine.Split(' ');

            priceTable.Add(new List<int>());

            foreach (string value in fileValues)
                priceTable[i].Add(int.Parse(value));
        }

        file.Close();



        // НАЧАЛО РЕШЕНИЯ

        int[,] transportationTable = new int[providerAmount, consumerAmount]; // таблица перевозок от производителя к потребителю
        long totalPrice = 0; // общая стоимость всех перевозок

        long totalNeed; // общая потребность всех потребителей
        do
        {
            // Шаг 1. Поиск наименьшей цены
            Task<int>[] tasks = new Task<int>[providerAmount];

            for (int i = 0; i < providerAmount; i++) // создаем задачу для каждой строки таблицы
            {
                int index = i;
                // учитываем только тех производителей, у которых остался товар
                tasks[index] = Task.Run(() => providerReserve[index] > 0 ? priceTable[index].Min() : int.MaxValue);
            }

            int[] taskResults = await Task.WhenAll(tasks); // результаты выполнения задач
            int lowerPrice = taskResults.Min(); // наименьшая цена перевозки

            int lowerPriceRow = Array.IndexOf(taskResults, lowerPrice); // индекс строки с наименьшей ценой
            int lowerPriceCol = priceTable[lowerPriceRow].IndexOf(lowerPrice); // индекс столбца с наименьшей ценой

            // Шаг 2. "Перевозка" товара
            int transportation = Math.Min(providerReserve[lowerPriceRow], consumerNeed[lowerPriceCol]); // наименьшее из значений среди запасов производителя и потребности потребителя

            transportationTable[lowerPriceRow, lowerPriceCol] = transportation; // записываем объем перевозок в таблицу перевозок

            providerReserve[lowerPriceRow] -= transportation; // списываем товар у поставщика
            consumerNeed[lowerPriceCol] -= transportation; // уменьшаем потребность у потребителя

            totalPrice += priceTable[lowerPriceRow][lowerPriceCol] * transportationTable[lowerPriceRow, lowerPriceCol]; // увеличиваем общую стоимость всех перевозок

            priceTable[lowerPriceRow][lowerPriceCol] = int.MaxValue; // в таблице стоимостей меняем цену завершенной перевозки

            totalNeed = consumerNeed.Sum(); // подсчитываем остаточную общую потребность

        } while (totalNeed != 0); // пока не все потребности удовлетворены



        // РЕЗУЛЬТАТ

        using (StreamWriter writer = new StreamWriter("out.txt", false))
        {
            writer.WriteLine(totalPrice);

            for (int i = 0; i < providerAmount; i++)
            {
                for (int j = 0; j < consumerAmount; j++)
                    writer.Write($"{transportationTable[i, j]} ");
                writer.WriteLine();
            }
        }
    }
}