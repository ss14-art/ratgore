game-ticker-restart-round = Перезапуск раунда...
game-ticker-start-round = Раунд начинается...
game-ticker-start-round-cannot-start-game-mode-fallback = Не удалось запустить режим { $failedGameMode }! Запускаем { $fallbackMode }...
game-ticker-start-round-cannot-start-game-mode-restart = Не удалось запустить режим { $failedGameMode }! Перезапуск раунда...
game-ticker-start-round-invalid-map = Выбранная карта { $map } не подходит для игрового режима { $mode }. Игровой режим может не функционировать как задумано...
game-ticker-unknown-role = Неизвестный
game-ticker-delay-start = Начало раунда отложено на { $seconds } { $seconds ->
    [one] секунду
    [few] секунды
   *[other] секунд
}.
game-ticker-pause-start = Начало раунда было приостановлено.
game-ticker-pause-start-resumed = Отсчёт начала раунда возобновлён.
game-ticker-player-join-game-message = Добро пожаловать в Сектор Полумесяца. Держите глаза открытыми, паранойю высокой, а союзников близко. Возможно, вы переживёте это.
game-ticker-get-info-text =
    Добро пожаловать на сервер [color=white]Ratbite[/color]
    Текущий раунд: [color=white]#{ $roundId }[/color]
    Текущее количество игроков: [color=white]{ $playerCount }[/color]
    Текущий игровой режим: [color=white]{ $gmTitle }[/color]
    >[color=yellow]{ $desc }[/color]
game-ticker-get-info-preround-text =
    Добро пожаловать на сервер [color=white]Ratbite[/color]
    Текущий раунд: [color=white]#{ $roundId }[/color]
    Текущее количество игроков: [color=white]{ $playerCount }[/color] ([color=white]{ $readyCount }[/color] { $readyCount ->
        [one] готов
        [few] готовы
       *[other] готовы
    })
    Текущий игровой режим: [color=white]{ $gmTitle }[/color]
    >[color=yellow]{ $desc }[/color]
game-ticker-no-map-selected = [color=yellow]Карта ещё не выбрана![/color]
game-ticker-player-no-jobs-available-when-joining = При попытке присоединиться к игре не было доступных должностей.
game-ticker-welcome-to-the-station = Гадальный прилив отступил. Весь персонал должен возобновить свои обычные обязанности. Любой, кто подозревает Гадальное заражение, должен явиться в медбей.
# Displayed in chat to admins when a player joins
player-join-message = Игрок { $name } присоединился.
player-first-join-message = Игрок { $name } присоединился впервые.
# Displayed in chat to admins when a player leaves
player-leave-message = Игрок { $name } вышел.
latejoin-arrival-announcement = { $character } ({ $job }) { CONJUGATE-HAVE($entity) } прибыл на станцию!
latejoin-arrival-announcement-special = { $job } { $character } на палубе!
latejoin-arrival-sender = Станция
latejoin-arrivals-direction = Шаттл, который доставит вас на станцию, прибудет в ближайшее время.
latejoin-arrivals-direction-time = Шаттл, который доставит вас на станцию, прибудет через { $time }.
latejoin-arrivals-dumped-from-shuttle = Таинственная сила не даёт вам покинуть шаттл прибытия.
latejoin-arrivals-teleport-to-spawn = Таинственная сила телепортирует вас с шаттла прибытия. Удачной смены!
preset-not-enough-ready-players = Невозможно запустить { $presetName }. Требуется { $minimumPlayers } { $minimumPlayers ->
    [one] игрок
    [few] игрока
   *[other] игроков
}, а готовы { $readyPlayersCount }.
preset-no-one-ready = Невозможно запустить { $presetName }. Нет готовых игроков.
game-run-level-PreRoundLobby = Лобби перед раундом
game-run-level-InRound = В раунде
game-run-level-PostRound = После раунда
