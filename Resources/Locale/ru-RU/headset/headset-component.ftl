# Chat window radio wrap (prefix and postfix)
chat-radio-message-wrap = [color={ $color }]{ $channel } [font size=11][color={ $languageColor }][bold]{ $language }[/bold][/color][/font][bold]{ $name }[/bold] { $verb }, [color={ $messageColor }]"{ $message }"[/color][/color]
chat-radio-message-wrap-bold = [color={ $color }]{ $channel } [font size=11][color={ $languageColor }][bold]{ $language }[/bold][/color][/font][bold]{ $name }[/bold] { $verb }, [color={ $messageColor }]"{ $message }"[/color][/color]
examine-headset-default-channel =
    Канал, использующийся этой гарнитурой по умолчанию - [color={ $color }]{ $channel ->
        [Syndicate] Синдикат
        [Supply] Снабжение
        [Command] Командование
        [CentCom] ЦентКом
        [Common] Длинная волна 2,5 км
        [Engineering] Инженерный
        [Science] Научный
        [Medical] Медицинский
        [Security] Безопасность
        [Service] Сервисный
       *[other] _
    }[/color].
chat-radio-common = Длинная волна 2,5 км
chat-radio-centcom = ЦентКом
chat-radio-command = Командование
chat-radio-engineering = Инженерный
chat-radio-medical = Медицинский
chat-radio-science = Научный
chat-radio-security = Безопасность
chat-radio-service = Сервисный
chat-radio-supply = Снабжение
chat-radio-syndicate = Синдикат
chat-radio-freelance = Наемный
# not headset but whatever
chat-radio-handheld = Портативный
chat-radio-binary = Бинарный
