# Публикация FajrApp в Microsoft Store

## Шаг 1: Регистрация разработчика

1. Перейдите на [Microsoft Partner Center](https://partner.microsoft.com/dashboard)
2. Войдите с Microsoft аккаунтом
3. Оплатите регистрацию:
   - **Физические лица:** ~$19 (единоразово)
   - **Компании:** ~$99 (единоразово)

## Шаг 2: Подготовьте иконки

Создайте PNG изображения и поместите в папку `FajrApp.Package/Images/`:

| Файл | Размер | Описание |
|------|--------|----------|
| `StoreLogo.png` | 50×50 px | Логотип в Store |
| `Square44x44Logo.png` | 44×44 px | Иконка в taskbar |
| `SmallTile.png` | 71×71 px | Маленькая плитка |
| `Square150x150Logo.png` | 150×150 px | Средняя плитка |
| `Square310x310Logo.png` | 310×310 px | Большая плитка |
| `Wide310x150Logo.png` | 310×150 px | Широкая плитка |

### Создание иконок из fajr.ico

Можно использовать онлайн-конвертеры:
- [icoconvert.com](https://icoconvert.com/)
- [resizeimage.net](https://resizeimage.net/)

## Шаг 3: Создайте MSIX пакет (Visual Studio 2022)

### Вариант A: Через Visual Studio

1. Откройте `FajrApp.sln` в Visual Studio 2022
2. **File → Add → Existing Project** → выберите `FajrApp.Package\FajrApp.Package.wapproj`
3. Правый клик на `FajrApp.Package` → **Set as Startup Project**
4. Правый клик на `FajrApp.Package` → **Publish** → **Create App Packages**
5. Выберите **Microsoft Store as a new app name**
6. Войдите в Partner Center и свяжите с приложением
7. Выберите архитектуру (x64) и создайте пакет

### Вариант B: MSIX Packaging Tool (проще)

1. Установите **MSIX Packaging Tool** из Microsoft Store
2. Запустите и выберите **Application package**
3. Укажите установщик: `publish\install\FajrApp-Setup-1.0.0.exe`
4. Следуйте мастеру для создания MSIX

## Шаг 4: Загрузите в Partner Center

1. Войдите в [Partner Center](https://partner.microsoft.com/dashboard)
2. Перейдите в **Apps and games** → **New product** → **MSIX or PWA app**
3. Зарезервируйте имя приложения: `FajrApp`
4. Заполните информацию:
   - **Описание** на всех поддерживаемых языках (EN, RU, ES, AR, ID, KK)
   - **Скриншоты** (минимум 1, рекомендуется 4-5)
   - **Категория:** Productivity или Lifestyle
   - **Возрастной рейтинг:** Для всех
5. Загрузите MSIX пакет
6. Нажмите **Submit to the Store**

## Шаг 5: Сертификация

- Обычно занимает **1-3 рабочих дня**
- Microsoft проверяет:
  - Безопасность приложения
  - Соответствие политикам Store
  - Корректность метаданных
- После одобрения приложение появится в Store

## Требования Microsoft Store

### Технические требования
- ✅ .NET 8.0 (поддерживается)
- ✅ WPF приложение (поддерживается)
- ✅ Интернет-доступ (нужна capability `internetClient`)

### Политики контента
- ✅ Религиозные приложения разрешены
- ✅ Бесплатные приложения разрешены
- ⚠️ Убедитесь, что нет нарушений авторских прав

## Полезные ссылки

- [Microsoft Partner Center](https://partner.microsoft.com/dashboard)
- [Документация по MSIX](https://docs.microsoft.com/windows/msix/)
- [Политики Microsoft Store](https://docs.microsoft.com/windows/uwp/publish/store-policies)
- [MSIX Packaging Tool](https://www.microsoft.com/store/productId/9N5LW3JBCXKF)

## Структура проекта для Store

```
FajrApp/
├── FajrApp.Package/
│   ├── FajrApp.Package.wapproj    # Проект упаковки
│   ├── Package.appxmanifest        # Манифест приложения
│   └── Images/                     # Иконки для Store
│       ├── StoreLogo.png
│       ├── Square44x44Logo.png
│       ├── SmallTile.png
│       ├── Square150x150Logo.png
│       ├── Square310x310Logo.png
│       └── Wide310x150Logo.png
└── build-msix.ps1                  # Скрипт сборки
```

## Обновление приложения

После публикации, для обновления:

1. Измените версию в `Package.appxmanifest`:
   ```xml
   <Identity ... Version="1.0.1.0" />
   ```
2. Пересоберите MSIX пакет
3. Загрузите новую версию в Partner Center
4. Отправьте на сертификацию

---

**Примечание:** Первая публикация может занять больше времени из-за более тщательной проверки.
