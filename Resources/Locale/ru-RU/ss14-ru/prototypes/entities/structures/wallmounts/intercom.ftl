ent-BaseIntercom = интерком
    .desc = Интерком. Для тех случаев, когда станции просто нужно что-то знать.
ent-IntercomAssembly = каркас интеркома
    .desc = Интерком. Не выглядит очень полезным прямо сейчас.
ent-IntercomConstructed = { ent-BaseIntercom }
    .suffix = Пустой, Открытая панель
    .desc = { ent-BaseIntercom.desc }
ent-Intercom = { ent-IntercomConstructed }
    .desc = { ent-IntercomConstructed.desc }
ent-BaseIntercomSecure = { ent-Intercom }
    .desc = { ent-Intercom.desc }
ent-IntercomCommon = { ent-Intercom }
    .suffix = Общий
    .desc = { ent-Intercom.desc }
ent-IntercomCommand = { ent-BaseIntercomSecure }
    .desc = Интерком. Он был укреплён металлом.
    .suffix = Командный
ent-IntercomEngineering = { ent-Intercom }
    .suffix = Инженерный
    .desc = { ent-Intercom.desc }
ent-IntercomMedical = { ent-Intercom }
    .suffix = Медицинский
    .desc = { ent-Intercom.desc }
ent-IntercomScience = { ent-Intercom }
    .suffix = Научный
    .desc = { ent-Intercom.desc }
ent-IntercomSecurity = { ent-BaseIntercomSecure }
    .desc = Интерком. Он был укреплён металлом из шлемов службы безопасности, из-за чего открыть его было очень сложно.
    .suffix = Безопасность
ent-IntercomService = { ent-Intercom }
    .suffix = Сервис
    .desc = { ent-Intercom.desc }
ent-IntercomSupply = { ent-Intercom }
    .suffix = Снабжение
    .desc = { ent-Intercom.desc }
ent-IntercomAll = { ent-Intercom }
    .suffix = Все
    .desc = { ent-Intercom.desc }
ent-IntercomFreelance = { ent-Intercom }
    .suffix = Фриланс
    .desc = { ent-Intercom.desc }
