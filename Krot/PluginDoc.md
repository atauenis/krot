﻿Документация по плагинам находится в начальной стадии разработки. Конечный текст будет на английском.
Пример API плагинов файловых систем и документация по коммандам fs* находятся в KrotLocalFSPlugin.cs (в одноимённой папке пректа).

Creating plug-in modules
========================

Плагины это просто DLL с классом, имплементирующим интерфейс IKrotPlugin. Сам интерфейс пишется голым текстом, без привязки к конкретной сборке. Крот при загрузке проверяет только факт наследования от некого интерфейса с таким именем. Набор функций интерфейса ограничен, и не планируется к изменению в разных версиях программы. Разнообразие функций обеспечивается через команды.

Общение "Крота" с плагинами идёт путём вызова команд и сообщения аргументов. Выполненность оценивается по коду возврата, результат (если нужен) возвращается через переменную, которая сообщается как опциональный аргумент Result.

Передача информации от плагина "Кроту" осуществляется через делегаты, которые хост сопоставляет в себе нужным местам (плагину нужно только обращаться к ним по мере необходимости).


Return codes for Krot 1.0.1702
------------------------------
* 0 = Okay
* 1 = Non-critical error. An argument has been not recongized by plug-in module.
* 2 = Critical error. The command name or an important argument has been not recongized.
* 3 = reserved
* 4 = An exception occured.
* Any other = see [currently not written] documentation/wiki.