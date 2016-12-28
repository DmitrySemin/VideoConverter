# Изотоп

##О системе
	Для распределенного транскодирования видео логично реализовать паттерн actor model. Проблема в том, что под выбранный стек (.NET Core) еще не портированы основные фреймворки реализующие этот паттерн (Akka.NET например). В свою очередь фреймворк от MS (Azure Service Fabric) пока не поддерживает установку никуда кроме серверных версий Windows. Соответственно пришлось реализовывать actor model самому. Испытывал очень большой соблазн реализовать взаимодействие акторов через стандартные Web API контроллеры, но отказался в пользу каcтомного роутинга и обработки сообщений из-за больших возможностей по кастомизации.

##Структура
	Разбил решение на 4 структурных модуля: 
- Converter - конвертер (обертка над ffmpeg). Не стал внедрять через IoC т.к. по условию используем конкретный конвертер. Но можно легко сделать при расширении набора конвертеров/форматов для обработки.
- Common - основная библиотека классов. Описаны все модели, акторы,ноды, кластер.
- Cluster - центральное asp.net приложение в котором поднимается кластер. Также визуализирует состояние кластера, нод и граф подключенных нод.
- Node - сервис для хостинга нод на машинах сотрудников.

##Запуск и отладка
- Задача реализована на ASP.NET Core 1.0.1. 
- Для отладки необходима VS 2015 (https://www.visualstudio.com/ru/downloads/) и ASP.NET Core SDK (https://www.microsoft.com/net/download/core). Версия SDK - 1.0.0-preview2-00313
- Для удобства отладки в окружении Windows добавил в решение 3 однотипных проекта ноды - чтобы стартовать их из студии вместе с кластером одновременно (IISExpress+Kestrel или просто Kestrel, не суть важно).

##Конфигурация и запуск
	Конфигурация кластера и ноды осуществляется через appsettings.json:
	- Кластер:
	-- InputPath - путь к хранилищу с видео (предполагаем что оно смонтировано под одинаковому пути на всех машинах сети ),
	-- OutputPath - путь к хранилищу с обработанным видео (предполагаем что оно смонтировано под одинаковому пути на всех машинах сети ).
	- Нода:
	-- FfMpegPath - путь к исполняемым файлам ffmpeg,
	-- TempDirectory - временная папка для обработки файлов
	-- ClusterUrl - путь к развернутому кластеру (например http://localhost:9000)
	-- ActorsCount - количество акторов, одновременно запускаемых на данной ноде

##Визуализация:
	Не стал тратить время на равернутый UX. Вывожу объемы, скорость, время, информацию по нодам. Подключившиеся и потом отвалившиеся ноды так же выводятся. Отдельно рисую топологию в виде графа:
	
Было желание прикрутить SignalR, но не хватило времени. Официально SignalR не поддерживается в ASP.NET Core. Неофициально возможно все. Например тут: https://radu-matei.github.io/blog/aspnet-core-mvc-signalr/ или тут https://chsakell.com/2016/10/10/real-time-applications-using-asp-net-core-signalr-angular/. 