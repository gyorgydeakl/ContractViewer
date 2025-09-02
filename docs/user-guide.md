A POC kipróbálásához be kell jelentkezni. Bejelentkezéshez navigáljunk a böngészőben a localhost:4200/login oldalra, majd töltsük ki a login formot! A létező felhasználónevek: alice, bob, carol, dave, eve. A jelszó minden usernek Admin_123.

Bejelentkezés után a cache-be kerül a bejelentekezett userhez tartozó user id (Pontosabban a bejelentkezést követő első olyan művelet végrehajtásakor, amihez kell a user id). Ezt meg is tudjuk nézni a jobb felső sarokban található 'View Cache' gomb megnyomásával, a jobb felső sarokban. Itt tetszőlegesen tudunk törölni cache adatokat és frissíteni a cache-t.

## Contractok
A bejelentkezett felhasználó szerződései lekérdezésre kerülnek a `/contracts` oldal megnyitásakor. Ilyenkor a cache tartalma megfelelően frissül.

Az adott user-hez lehet véletlenszerűen generálni szerődéseket, hogy könnyen fel lehessen tölteni adattal. Ehhez, a '+' gombra kell rányomnunk a szerződések listázásakor.

## Meghatalmazások
A `/poas` oldalon lehet lekérdezni az adott felhasználó összes meghatalmazását. A jobb fenti dropdown-ban lehet kiválasztani, hogy az olyanokat lássunk, ahol a bejelenkezett user a meghatalmazó, vagy olyanokat, ahol a meghatalmazott.

A poa listázás után a cache tartalma is frissül.

Meghatalmazásokat lehet generálni is. Ehhez, a '+' gombra kell rányomnunk a dokumentumok listázásakor.

## Cache módosítása más service-ek nevében.
A `/cache` oldalon lehet a cache tartalmát tetszőlegesen módosítani több service nevében. Jobb fent ki lehet választani, hogy melyik service nevében akarunk cselekedni. Ezek után a cache csak az adott service-nek megfelelő előtagú kulcsokat mutatja, és csak ilyen előtaggal enged felvenni kulcsokat. 

Az 'Admin' kiválasztásával teetszőlegesen bármely kulcsot lehet kezelni, illetve bármilyen formájú kulcsot fel lehet venni.