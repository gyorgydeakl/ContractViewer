A POC kipróbálásához be kell jelentkezni. Bejelentkezéshez navigáljunk a böngészőben a localhost:4200/login oldalra, majd töltsük ki a login formot! A létező felhasználónevek: alice, bob, carol, dave, eve. A jelszó minden usernek Admin_123.

Bejelentkezés után a cache-be kerül a bejelentekezett userhez tartozó user id (Pontosabban a bejelentkezést követő első olyan művelet végrehajtásakor, amihez kell a user id). Ezt meg is tudjuk nézni a cache editorban, a jobb felső sarokban. Itt tetszőlegesen tudunk törölni cache adatokat és frissíteni a cache-t.

## Contractok
A bejelentkezett felhasználó szerződései lekérdezésre kerülnek a `/contracts` oldal megnyitásakor. Ilyenkor a cache tartalma megfelelően frissül.

Az adott user-hez lehet véletlenszerűen generálni szerődéseket, hogy könnyen fel lehessen tölteni adattal. Ehhez, a '+' gombra kell rányomnunk a szerződések listázásakor.

## Dokumentumok
A `/documents` oldalon lehet lekérdezni az adott felhasználó összes szerződéséhez tartozó dokumentumot.

A dokumentumok a lekérdezésük után a cache tartalma megfelelően frissül.

Az adott user-hez dokumentumokat is lehet véletlenszerűen generálni. Ehhez, a '+' gombra kell rányomnunk a dokumentumok listázásakor.