# Использование

## Введение

Изначально этот бот создавался, чтобы использоваться сразу несколькими разными людьми - кто угодно может создать неограниченное количество чатов и соединить их между собой. Если вам нужно именно такое поведение, включите параметр `public`, т.к. по умолчанию он выключен.

## Контроль доступа

Для контроля доступа используется достаточно примитивная система на основе уровней доступа. <br/>
Ниже представлены все 4 возможных уровня доступа и их описание.

| Уровень | Идентификатор | Название      | Описание                                                       |
| ------- | ------------- | ------------- | -------------------------------------------------------------- |
| 0       | Owner         | Владелец      | Может назначать администраторов                                |
| 1       | Admin         | Администратор | Может изменять настройки чата, создавать между чатами маршруты |
| 2       | Moderator     | Модератор     | Может блокировать пользователей                                |
| 3       | None          | Пользователь  | Ничего не может                                                |

Для изоляции незнакомых пользователей уровень доступа применяется локально для каждого чата. Это значит, что один и тот же пользователь в одном чате может быть администратором, а в другом обычным пользователем.

После приглашения бота в беседу необходимо инициализировать чат. Это делается командой `/c init`. Пользователь, первый выполнивший инициализацию, назначается администратором. Владельцем назначается реальный владелец беседы или администратор, если реального владельца определить не удалось. Подразумевается, что пользователь, получивший права администратора или владельца, знает что с ними делать дальше. Повторные попытки инициализировать чат будут игнорироваться.<br/>
Информацию о чате можно получить с помощью команды `/c info`.

Для создания маршрута пользователь должен быть администратором во всех чатах, которые он хочет связать. <br/>

```
/r sync <chat1> <chat2> ... <chatN> # Создать маршрут
```

Обратите внимание, что в одном маршруте могут находиться два и более чатов из разных соцсетей и мессенджеров. <br/>
Для удаления маршрута используйте `/r desync`, а для исключения чата из маршрута `/r exclude`.

Для получения информации об остальных командах существует `/help`.

## Обработка параметров

В большинстве случаев, команды запрашивают такие параметры как чат, маршрут и пользователь. Для указания любого из этих объектов можно использовать глобальный идентификатор в базе данных. <br/>
Для упрощения можно использовать:

-   `this` или `here` для текущего чата;
-   Имя пользователя или идентификатор одного из его аккаунтов;
-   Идентификатор или порядковый номер уровня доступа.

Например, мы устанавливаем права пользователю в текущем чате:

```
/u scope this <username> Moderator
# или
/u scope here <username> 1
```