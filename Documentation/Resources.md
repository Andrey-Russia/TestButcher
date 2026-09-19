# Ресурсы и структура проекта

## Источники

- [Техническое задание](https://docs.google.com/document/d/1MpV-Qq4mJNTBkj46c8L6qKUHXL-U4XLO/edit) — текст сохранён в `References/Task.txt`.
- [Run Rich 3D — игра-референс](https://play.google.com/store/apps/details?id=com.ohmgames.richtopoor).
- [Видео ожидаемого результата](https://drive.google.com/file/d/1Vd_fSF3d7_NdNNMYrIn2T7u78GZeGwrl/view) — скачано в `References/IMG_3474.MP4`; визуальный разбор видео не выполнялся.
- [Папка ассетов](https://drive.google.com/drive/folders/1l9OVXK81nPCKHjT59URhf8ZcPyd9A3l0) — скачаны все четыре файла.
- [Менеджер уровней](https://drive.google.com/file/d/1u7xLOmAnrZ65ECHbE-90GT6ffS7syHGP/view) — распакован с сохранением исходных GUID.

## Размещение

```text
Assets/
  _Project/
    Scenes/
    Scripts/
      Core/ Player/ Input/ Camera/ Levels/ UI/ Tutorial/ Audio/ VFX/
      Gameplay/Pickups/ Obstacles/ Gates/ Finish/
    Prefabs/
      Player/ Levels/ Environment/ Pickups/ Obstacles/ Gates/ Finish/ UI/ VFX/
    Animations/Clips/ Controllers/
    Art/Materials/ Textures/ Models/
    Audio/Music/ SFX/
    UI/Fonts/ Sprites/ Atlases/
    VFX/
    Data/Levels/ Player/ Balance/
    Settings/
    Editor/
  ThirdParty/
    RunRich/Sounds/AudioClip/
    RunRich/Visual/Fonts/ Material/ Mesh/ Shader/ Sprite/ Texture2D/
    ButchersGames/LevelManager/Editor/
Documentation/References/
Downloads/SourceAssets/
```

Папки `_Project` подготовлены для разработки; игровой код, сцена и уровень ещё не реализованы. Существующие Scenes, Settings, TutorialInfo и InputSystem_Actions сохранены на прежних местах. Для новых папок созданы `.meta`.

## Содержимое

Из Assets.rar добавлены все 1 539 файлов: 1 017 мешей в `.asset`, 178 спрайтов в `.asset`, ещё один `.asset` с данными материала, 6 FBX, 225 PNG, 74 `.mat`, 23 OGG, 7 шейдеров, 4 атласа и 4 TTF. Исходные файлы скопированы без изменений.

BG_LevelManager.unitypackage и Level Manager.zip содержат идентичные четыре C#-файла. Импортирована только версия unitypackage, чтобы исключить дублирование классов; оба оригинальных архива сохранены в Downloads/SourceAssets. Единственное изменение импортированного кода — `using UnityEditor` в LevelManager.cs заключён в `#if UNITY_EDITOR` для совместимости с player-сборкой. Логика предоставленного менеджера не перерабатывалась.

## Ограничения исходных данных и проверки

- В Assets.rar нет ни одного исходного `.meta`: ссылки на текстуры, шейдеры и другие ресурсы используют GUID, которые невозможно достоверно восстановить только по имени файла. Unity создаст новые метаданные при импорте, но это не восстановит старые ссылки. Список неразрешённых ссылок сохранён в `References/UnresolvedSourceReferences.csv`.
- Проект использует URP. Совместимость исходных шейдеров и материалов с URP не подтверждена; после восстановления ссылок потребуется визуальная проверка и, возможно, адаптация материалов.
- В архиве нет отдельных `.anim`, `.controller`, `.prefab` и игровых сцен. Наличие встроенных анимаций и рига в FBX требует проверки в Unity.
- Видео и исходные архивы хранятся вне Assets, чтобы не импортировать их в игровой билд.
- Проверено побайтовое совпадение импортированных ресурсов с распакованным архивом. Контрольные SHA-256 скачанных файлов сохранены в `References/DownloadHashes.json`.
- Попытка проверки Unity 6000.3.17f1 в batchmode остановилась: редактор сообщил, что проект уже открыт другим экземпляром. Успешная компиляция и импорт редактором не подтверждены. Лог: `Logs/ResourceImport.log`.

Чтобы восстановить исходные связи без ручного переназначения, нужен повторный экспорт ассетов вместе с `.meta` или полный `.unitypackage` от автора задания.
