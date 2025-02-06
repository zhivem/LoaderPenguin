# LoaderPenguin

<img src="https://github.com/zhivem/Loader-for-DPI-Penguin/blob/main/update_reset.ico" width=5% height=5%>

## Основной репозиторий
> [!IMPORTANT]
> Основной репозиторий программы: [DPI-Penguin](https://github.com/zhivem/DPI-Penguin)

## Loader
Данная утилита предназначена для обновления или переустановки приложения `DPI Penguin`. Программа запускает процесс обновления, загружая новые версии программы с облачного хранилища (в данном случае — Яндекс.Диск), извлекает обновление из ZIP-архива и перезапускает основное приложение.

![image](https://github.com/user-attachments/assets/990823de-6801-48e1-8d34-845c7e48d4ac)

## Важно
> [!IMPORTANT]
> ❗️Не удаляйте, не перемещайте, не меняйте название `loader.exe`. Утилита должна находится рядом с программой `DPI Penguin`❗️

## Описание ключевых этапов работы программы:
- **Запуск программы:** при запуске проверяется, есть ли права администратора. Если нет — инициируется перезапуск с правами администратора.
- **Инициализация обновления:** создается окно с графическим интерфейсом, где отображаются прогресс и сообщения о ходе обновления.
- **Загрузка обновления:** программа скачивает архив с новой версией программы с облачного хранилища.
- **Распаковка обновления:** после завершения загрузки архив распаковывается в нужную директорию.
- **Перезапуск приложения:** после успешного завершения обновления программа перезапускает основное приложение DPI Penguin.
- **Обработка ошибок:** если на любом этапе обновления возникает ошибка, программа сообщает об этом пользователю и предлагает инструкции по ручному обновлению через ссылки на GitHub.

## Благодарности

- **GoodbyeDPI:** Основа для работы YouTube. Разработчик: ValdikSS. [Репозиторий](https://github.com/ValdikSS/GoodbyeDPI)
- **Zapret:** Основа для работы Discord и YouTube. Разработчик: bol-van. [Репозиторий](https://github.com/bol-van/zapret)

## Лицензия 

Этот проект лицензирован под [Apache License, Version 2.0.](https://raw.githubusercontent.com/zhivem/DPI-Penguin/refs/heads/main/LICENSE.md)
