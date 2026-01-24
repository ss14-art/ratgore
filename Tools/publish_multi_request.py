#!/usr/bin/env python3

import requests
import os
import subprocess
from typing import Iterable
import urllib3
import sys
from datetime import datetime

urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

# Цвета для консоли
class Colors:
    HEADER = '\033[95m'
    OKBLUE = '\033[94m'
    OKCYAN = '\033[96m'
    OKGREEN = '\033[92m'
    WARNING = '\033[93m'
    FAIL = '\033[91m'
    ENDC = '\033[0m'
    BOLD = '\033[1m'

def log(message, level="INFO"):
    """Логирование с временной меткой и цветом"""
    timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    colors = {
        "INFO": Colors.OKBLUE,
        "SUCCESS": Colors.OKGREEN,
        "WARNING": Colors.WARNING,
        "ERROR": Colors.FAIL,
        "DEBUG": Colors.OKCYAN
    }
    color = colors.get(level, "")
    print(f"{color}[{timestamp}] [{level}]{Colors.ENDC} {message}")
    sys.stdout.flush()

# Получение переменных окружения
PUBLISH_TOKEN = os.environ.get("PUBLISH_TOKEN")
VERSION = os.environ.get("GITHUB_SHA")
RELEASE_DIR = "release"

# Конфигурация для вашего форка
ROBUST_CDN_URL = "https://cdn.ss14.art/"
FORK_ID = "ratgore14"

def main():
    log("=" * 80, "INFO")
    log("НАЧАЛО ПУБЛИКАЦИИ СБОРКИ", "INFO")
    log("=" * 80, "INFO")
    
    # Проверка переменных окружения
    log("Проверка переменных окружения...", "INFO")
    if not PUBLISH_TOKEN:
        log("ОШИБКА: PUBLISH_TOKEN не установлен!", "ERROR")
        sys.exit(1)
    log(f"✓ PUBLISH_TOKEN установлен (длина: {len(PUBLISH_TOKEN)} символов)", "SUCCESS")
    
    if not VERSION:
        log("ОШИБКА: GITHUB_SHA (VERSION) не установлен!", "ERROR")
        sys.exit(1)
    log(f"✓ VERSION: {VERSION}", "SUCCESS")
    
    log(f"✓ CDN URL: {ROBUST_CDN_URL}", "INFO")
    log(f"✓ FORK ID: {FORK_ID}", "INFO")
    log(f"✓ RELEASE DIR: {RELEASE_DIR}", "INFO")
    
    # Проверка директории с релизами
    log("", "INFO")
    log("Проверка директории релизов...", "INFO")
    if not os.path.exists(RELEASE_DIR):
        log(f"ОШИБКА: Директория {RELEASE_DIR} не найдена!", "ERROR")
        sys.exit(1)
    
    files = list(get_files_to_publish())
    if not files:
        log(f"ОШИБКА: В директории {RELEASE_DIR} не найдено .zip файлов!", "ERROR")
        sys.exit(1)
    
    log(f"✓ Найдено файлов для публикации: {len(files)}", "SUCCESS")
    total_size = sum(os.path.getsize(f) for f in files)
    log(f"✓ Общий размер: {total_size / (1024*1024):.2f} МБ", "SUCCESS")
    for f in files:
        size_mb = os.path.getsize(f) / (1024*1024)
        log(f"  - {os.path.basename(f)}: {size_mb:.2f} МБ", "DEBUG")
    
    # Получение версии движка
    log("", "INFO")
    log("Получение версии движка...", "INFO")
    engine_version = get_engine_version()
    log(f"✓ Engine Version: {engine_version}", "SUCCESS")
    
    # Создание сессии
    log("", "INFO")
    log("Создание HTTP сессии...", "INFO")
    session = requests.Session()
    session.headers = {
        "Authorization": f"Bearer {PUBLISH_TOKEN}",
    }
    session.verify = False
    log("✓ Сессия создана", "SUCCESS")
    
    # Тест соединения
    log("", "INFO")
    log("=" * 80, "INFO")
    log("ШАГ 1: ТЕСТ СОЕДИНЕНИЯ С CDN", "INFO")
    log("=" * 80, "INFO")
    test_url = f"{ROBUST_CDN_URL}fork/{FORK_ID}/"
    log(f"Отправка GET запроса: {test_url}", "INFO")
    
    try:
        test_resp = session.get(test_url, timeout=10)
        log(f"✓ Ответ получен", "SUCCESS")
        log(f"  Статус код: {test_resp.status_code}", "DEBUG")
        log(f"  Content-Type: {test_resp.headers.get('Content-Type', 'N/A')}", "DEBUG")
        log(f"  Content-Length: {test_resp.headers.get('Content-Length', 'N/A')}", "DEBUG")
        
        if test_resp.status_code == 200:
            log("✓ Соединение с CDN успешно установлено", "SUCCESS")
        else:
            log(f"⚠ Неожиданный статус код: {test_resp.status_code}", "WARNING")
            log(f"  Ответ: {test_resp.text[:200]}", "DEBUG")
    except Exception as e:
        log(f"✗ ОШИБКА соединения: {e}", "ERROR")
        log("Продолжаем несмотря на ошибку...", "WARNING")
    
    # Начало публикации
    log("", "INFO")
    log("=" * 80, "INFO")
    log("ШАГ 2: НАЧАЛО ПУБЛИКАЦИИ", "INFO")
    log("=" * 80, "INFO")
    
    start_url = f"{ROBUST_CDN_URL}fork/{FORK_ID}/publish/start"
    data = {
        "version": VERSION,
        "engineVersion": engine_version,
    }
    
    log(f"Отправка POST запроса: {start_url}", "INFO")
    log(f"Данные запроса:", "DEBUG")
    log(f"  version: {data['version']}", "DEBUG")
    log(f"  engineVersion: {data['engineVersion']}", "DEBUG")
    
    try:
        resp = session.post(start_url, json=data, timeout=30)
        log(f"✓ Ответ получен", "SUCCESS")
        log(f"  Статус код: {resp.status_code}", "DEBUG")
        
        if resp.status_code == 200:
            log("✓ Публикация успешно начата!", "SUCCESS")
            try:
                response_data = resp.json()
                log(f"  Ответ сервера: {response_data}", "DEBUG")
            except:
                log(f"  Ответ (текст): {resp.text[:200]}", "DEBUG")
        else:
            log(f"✗ ОШИБКА: Статус код {resp.status_code}", "ERROR")
            log(f"  Ответ сервера: {resp.text[:500]}", "ERROR")
            resp.raise_for_status()
    except requests.exceptions.HTTPError as e:
        log(f"✗ HTTP ОШИБКА: {e}", "ERROR")
        sys.exit(1)
    except Exception as e:
        log(f"✗ НЕОЖИДАННАЯ ОШИБКА: {e}", "ERROR")
        sys.exit(1)
    
    # Загрузка файлов
    log("", "INFO")
    log("=" * 80, "INFO")
    log("ШАГ 3: ЗАГРУЗКА ФАЙЛОВ", "INFO")
    log("=" * 80, "INFO")
    
    file_url = f"{ROBUST_CDN_URL}fork/{FORK_ID}/publish/file"
    
    for idx, file in enumerate(files, 1):
        file_name = os.path.basename(file)
        file_size = os.path.getsize(file)
        file_size_mb = file_size / (1024*1024)
        
        log("", "INFO")
        log(f"Файл {idx}/{len(files)}: {file_name}", "INFO")
        log(f"  Размер: {file_size_mb:.2f} МБ ({file_size:,} байт)", "DEBUG")
        log(f"  Путь: {file}", "DEBUG")
        
        try:
            with open(file, "rb") as f:
                headers = {
                    "Content-Type": "application/octet-stream",
                    "Robust-Cdn-Publish-File": file_name,
                    "Robust-Cdn-Publish-Version": VERSION
                }
                
                log(f"  Отправка POST запроса: {file_url}", "DEBUG")
                log(f"  Headers:", "DEBUG")
                log(f"    Robust-Cdn-Publish-File: {file_name}", "DEBUG")
                log(f"    Robust-Cdn-Publish-Version: {VERSION}", "DEBUG")
                log(f"  Начало загрузки...", "INFO")
                
                resp = session.post(file_url, data=f, headers=headers, timeout=300)
                
                log(f"  ✓ Ответ получен", "SUCCESS")
                log(f"    Статус код: {resp.status_code}", "DEBUG")
                
                if resp.status_code == 200:
                    log(f"  ✓ Файл {file_name} успешно загружен!", "SUCCESS")
                else:
                    log(f"  ✗ ОШИБКА: Статус код {resp.status_code}", "ERROR")
                    log(f"    Ответ сервера: {resp.text[:500]}", "ERROR")
                    resp.raise_for_status()
                    
        except requests.exceptions.ConnectionError as e:
            log(f"  ✗ ОШИБКА СОЕДИНЕНИЯ: {e}", "ERROR")
            log(f"  Возможная причина: таймаут или проблема с nginx конфигурацией", "ERROR")
            log(f"  Проверьте настройки client_max_body_size и proxy_read_timeout", "ERROR")
            sys.exit(1)
        except requests.exceptions.Timeout as e:
            log(f"  ✗ ТАЙМАУТ: {e}", "ERROR")
            log(f"  Файл слишком долго загружался (>300 секунд)", "ERROR")
            sys.exit(1)
        except Exception as e:
            log(f"  ✗ НЕОЖИДАННАЯ ОШИБКА: {e}", "ERROR")
            sys.exit(1)
    
    # Завершение публикации
    log("", "INFO")
    log("=" * 80, "INFO")
    log("ШАГ 4: ЗАВЕРШЕНИЕ ПУБЛИКАЦИИ", "INFO")
    log("=" * 80, "INFO")
    
    finish_url = f"{ROBUST_CDN_URL}fork/{FORK_ID}/publish/finish"
    data = {
        "version": VERSION
    }
    
    log(f"Отправка POST запроса: {finish_url}", "INFO")
    log(f"Данные запроса:", "DEBUG")
    log(f"  version: {data['version']}", "DEBUG")
    
    try:
        resp = session.post(finish_url, json=data, timeout=30)
        log(f"✓ Ответ получен", "SUCCESS")
        log(f"  Статус код: {resp.status_code}", "DEBUG")
        
        if resp.status_code == 200:
            log("✓ Публикация успешно завершена!", "SUCCESS")
            try:
                response_data = resp.json()
                log(f"  Ответ сервера: {response_data}", "DEBUG")
            except:
                log(f"  Ответ (текст): {resp.text[:200]}", "DEBUG")
        else:
            log(f"✗ ОШИБКА: Статус код {resp.status_code}", "ERROR")
            log(f"  Ответ сервера: {resp.text[:500]}", "ERROR")
            resp.raise_for_status()
    except Exception as e:
        log(f"✗ ОШИБКА ПРИ ЗАВЕРШЕНИИ: {e}", "ERROR")
        sys.exit(1)
    
    # Итоги
    log("", "INFO")
    log("=" * 80, "INFO")
    log("ПУБЛИКАЦИЯ ЗАВЕРШЕНА УСПЕШНО! 🎉", "SUCCESS")
    log("=" * 80, "INFO")
    log(f"Версия: {VERSION}", "INFO")
    log(f"Версия движка: {engine_version}", "INFO")
    log(f"Загружено файлов: {len(files)}", "INFO")
    log(f"Общий размер: {total_size / (1024*1024):.2f} МБ", "INFO")
    log("=" * 80, "INFO")


def get_files_to_publish() -> Iterable[str]:
    """Получение списка файлов для публикации"""
    for file in os.listdir(RELEASE_DIR):
        if file.endswith('.zip'):
            yield os.path.join(RELEASE_DIR, file)


def get_engine_version() -> str:
    """Получение версии движка из RobustToolbox"""
    try:
        proc = subprocess.run(
            ["git", "describe", "--tags", "--abbrev=0"],
            stdout=subprocess.PIPE,
            cwd="RobustToolbox",
            check=True,
            encoding="UTF-8"
        )
        tag = proc.stdout.strip()
        if tag.startswith("v"):
            return tag[1:]  # Убираем префикс v
        return tag
    except subprocess.CalledProcessError as e:
        log(f"⚠ Не удалось получить версию движка через git: {e}", "WARNING")
        log(f"  Используется версия по умолчанию: 'unknown'", "WARNING")
        return "unknown"
    except Exception as e:
        log(f"⚠ Неожиданная ошибка при получении версии движка: {e}", "WARNING")
        log(f"  Используется версия по умолчанию: 'unknown'", "WARNING")
        return "unknown"


if __name__ == '__main__':
    try:
        main()
    except KeyboardInterrupt:
        log("", "INFO")
        log("⚠ Публикация прервана пользователем", "WARNING")
        sys.exit(1)
    except Exception as e:
        log("", "INFO")
        log(f"✗ КРИТИЧЕСКАЯ ОШИБКА: {e}", "ERROR")
        import traceback
        log(traceback.format_exc(), "ERROR")
        sys.exit(1)
