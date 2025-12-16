command-list-langs-desc = Показать языки, на которых ваша сущность может говорить в данный момент.
command-list-langs-help = Использование: { $command }
command-saylang-desc = Отправить сообщение на определённом языке. Для выбора языка можно использовать название языка или его позицию в списке языков.
command-saylang-help = Использование: { $command } <id языка> <сообщение>. Пример: { $command } TauCetiBasic "Привет, мир!". Пример: { $command } 1 "Привет, мир!"
command-language-select-desc = Выбрать текущий разговорный язык вашей сущности. Можно использовать название языка или его позицию в списке языков.
command-language-select-help = Использование: { $command } <id языка>. Пример: { $command } 1. Пример: { $command } TauCetiBasic
command-language-spoken = Разговорные:
command-language-understood = Понимаемые:
command-language-current-entry = { $id }. { $language } - { $name } (текущий)
command-language-entry = { $id }. { $language } - { $name }
command-language-invalid-number = Номер языка должен быть от 0 до { $total }. Альтернативно используйте название языка.
command-language-invalid-language = Язык { $id } не существует или вы не можете на нём говорить.

# toolshed

command-description-language-add = Добавляет новый язык переданной сущности. Два последних аргумента указывают, должен ли он быть разговорным/понимаемым. Пример: 'self language:add "Canilunzt" true true'
command-description-language-rm = Удаляет язык у переданной сущности. Работает аналогично language:add. Пример: 'self language:rm "TauCetiBasic" true true'.
command-description-language-lsspoken = Показывает все языки, на которых сущность может говорить. Пример: 'self language:lsspoken'
command-description-language-lsunderstood = Показывает все языки, которые сущность может понимать. Пример: 'self language:lssunderstood'
command-description-translator-addlang = Добавляет новый целевой язык переданной сущности-переводчику. См. language:add для подробностей.
command-description-translator-rmlang = Удаляет целевой язык у переданной сущности-переводчика. См. language:rm для подробностей.
command-description-translator-addrequired = Добавляет новый обязательный язык переданной сущности-переводчику. Пример: 'ent 1234 translator:addrequired "TauCetiBasic"'
command-description-translator-rmrequired = Удаляет обязательный язык у переданной сущности-переводчика. Пример: 'ent 1234 translator:rmrequired "TauCetiBasic"'
command-description-translator-lsspoken = Показывает все разговорные языки для переданной сущности-переводчика. Пример: 'ent 1234 translator:lsspoken'
command-description-translator-lsunderstood = Показывает все понимаемые языки для переданной сущности-переводчика. Пример: 'ent 1234 translator:lssunderstood'
command-description-translator-lsrequired = Показывает все обязательные языки для переданной сущности-переводчика. Пример: 'ent 1234 translator:lsrequired'
command-language-error-this-will-not-work = Это не сработает.
command-language-error-not-a-translator = Сущность { $entity } не является переводчиком.
