import typing
import logging
import os

from file import FluentFile
from fluentast import FluentAstAbstract
from fluentformatter import FluentFormatter
from fluent.syntax import ast, FluentParser, FluentSerializer


# ======================== НАСТРОЙКИ ПУТЕЙ ========================

# Укажи реальные пути к папкам локализаций en-US и ru-RU
EN_DIR = r'D:\SpaceStation14Server\Ratbite\RatbiteLocale2\Ratbite\Resources\Locale\Re'
RU_DIR = r'D:\SpaceStation14Server\Ratbite\RatbiteLocale2\Ratbite\Resources\Locale\ru-RU'


# ========================= ЛОГИКА СКРИПТА ========================

class KeyValueSync:
    """Заменяет значения ключей из en-US в ru-RU, пары ищутся по замене 'en-US' -> 'ru-RU' в пути."""

    def __init__(self, en_dir: str, ru_dir: str):
        self.en_dir = os.path.abspath(en_dir)
        self.ru_dir = os.path.abspath(ru_dir)
        self.changed_files: typing.List[FluentFile] = []
        self.parser = FluentParser()
        self.serializer = FluentSerializer(with_junk=True)

    def execute(self) -> typing.List[FluentFile]:
        self.changed_files = []

        en_files = self._collect_fluent_files(self.en_dir)
        logging.info(f'Найдено английских файлов: {len(en_files)}')

        for en_path in en_files:
            # строим путь к ru-файлу заменой корня en-US на ru-RU
            rel = os.path.relpath(en_path, self.en_dir)
            ru_path = os.path.join(self.ru_dir, rel)

            if not os.path.exists(ru_path):
                continue

            en_file = FluentFile(en_path)
            ru_file = FluentFile(ru_path)
            self._sync_file(en_file, ru_file)

        return self.changed_files

    def _collect_fluent_files(self, root: str) -> typing.List[str]:
        result = []
        for dirpath, _, filenames in os.walk(root):
            for name in filenames:
                if name.lower().endswith('.ftl'):
                    result.append(os.path.join(dirpath, name))
        return result

    def _sync_file(self, en_file: FluentFile, ru_file: FluentFile):
        """Заменяет значения и атрибуты совпадающих ключей ru значениями из en."""
        en_ast = self.parser.parse(en_file.read_data())
        ru_ast = self.parser.parse(ru_file.read_data())

        en_map = {}
        for msg in en_ast.body:
            if self._is_comment(msg):
                continue
            key = FluentAstAbstract.get_id_name(msg)
            if key:
                en_map[key] = msg

        replaced = 0

        for ru_msg in ru_ast.body:
            if self._is_comment(ru_msg):
                continue
            key = FluentAstAbstract.get_id_name(ru_msg)
            if not key:
                continue

            en_msg = en_map.get(key)
            if not en_msg:
                continue

            # заменяем value
            ru_msg.value = en_msg.value

            # заменяем атрибуты
            if en_msg.attributes:
                ru_msg.attributes = en_msg.attributes
            elif ru_msg.attributes:
                ru_msg.attributes = []

            replaced += 1

        if replaced:
            new_content = self.serializer.serialize(ru_ast)
            ru_file.save_data(new_content)
            logging.info(
                f'{ru_file.full_path}: заменено {replaced} значений ключей (источник {en_file.full_path})'
            )
            self.changed_files.append(ru_file)

    def _is_comment(self, node) -> bool:
        return isinstance(node, (ast.ResourceComment, ast.GroupComment, ast.Comment))


# ========================= ЗАПУСК СКРИПТА ========================

logging.basicConfig(
    level=logging.INFO,
    format='[%(levelname)s] %(message)s'
)

print('Запуск синхронизации ЗНАЧЕНИЙ ключей...')
print('Приоритет: en-US → ru-RU')
print('Логика: значения ключей из en-US заменяют значения в ru-RU (по относительному пути en-US → ru-RU)\n')

print('EN_DIR =', EN_DIR)
print('RU_DIR =', RU_DIR)
print('EN_DIR существует:', os.path.isdir(EN_DIR))
print('RU_DIR существует:', os.path.isdir(RU_DIR))

sync = KeyValueSync(EN_DIR, RU_DIR)
changed_files = sync.execute()

if changed_files:
    print(f'\nФорматирование {len(changed_files)} изменённых файлов...')
    FluentFormatter.format(changed_files)
    print('Готово.')
else:
    print('Файлы не изменялись (не найдено пар en-US → ru-RU или нет совпадающих ключей).')

print('Скрипт завершён.')
input('\nНажмите Enter, чтобы выйти...')