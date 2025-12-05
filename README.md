# NRL-prosjekt-Gr14-2025H  
Semesterprosjekt for Kartverket og Norsk Luftambulanse, høst 2025 (gruppe 14).

Dette repoet inneholder en ASP.NET Core MVC-applikasjon for å registrere, håndtere og kvalitetssikre luftfartshindre.  
Piloter kan melde inn hindere via et kart, og registerførere kan se, vurdere og godkjenne/avvise innmeldingene.  
Applikasjon, database og lokal test-epost (Mailhog) kjører sammen i Docker, med MariaDB som database.

---

## 1. Kom i gang

#### 1) Klon prosjektet

git clone https://github.com/aanonho/NRL-prosjekt-Gr14-2025H.git
cd NRL-prosjekt-Gr14-2025H

### 2) Sett opp miljøvariabler (.env)
I rotmappen ligger det en malfil: **.env.example**

Bruk denne til å lage din egen lokale .env: **cp .env.example .env**

Åpne deretter .env og fyll inn malen for tilkobling.
Du kan bruke standardverdier, som skolen har lagt ut, eller lage dine egne.
Poenget er at alle som utvikler eller skal kjøre systemet putter inn verdier her.

**Hvorfor har vi gjort det slik?** På grunn av:
- Sikkerhet: Passord og andre hemmelige ting ligger ikke i kildekode eller docker-compose.yml, og blir ikke lagt ut på GitHub. De finnes nå kun i lokale .env-filer.
- Det er god praksis: Bruk av miljøvariabler og .env er standard praksis for webapplikasjoner og Docker. Dette gjør det enklere å kjøre samme kode i ulike miljøer.
Fleksibilitet
- Hver utvikler (eller sensor) kan bruke egne lokale passord/brukere uten å endre kode eller få passord.
- Enklere vedlikehold: Endringer i databasekonfigurasjon gjøres i én fil (.env), i stedet for i flere ulike configfiler (docker-compose.yml, appsettings*.json, osv.).
 
 Dette er kanskje ikke nødvendig i et lite skoleprosjekt, vi ville vise at vi tenker på sikkerhet og vet hva som er god praksis.

### Kom i gang - Steg 3 Start systemet (web + database)

Fra rotmappen (der docker-compose.yml ligger):
docker compose up --build

**Hva som skjer da:**
- Starter en MariaDB-databasecontainer (db) med brukere/passord fra .env.
- Starter webapplikasjonen (WebApplication1) og kobler den mot databasen via connection string som leses fra ConnectionStrings__DefaultConnection (som igjen er basert på miljøvariabler og MYSQL_*).
- Starter en Mailhog-container for lokal e-post i utvikling. Mailhog brukes kun til å teste «glemt passord»-funksjonen og er tilgjengelig på http://localhost:8025.
- Oppretter en navngitt volume for database-data: nrl-db-data.

Når alt er oppe, er applikasjonen tilgjengelig på:

**http://localhost:8080**

## Systemarkitektur – hvordan systemet er bygget

### Lagdeling og hovedkomponenter

Systemet følger en klassisk lagdelt struktur oppå ASP.NET Core MVC:
### Presentasjonslag (UI): 
  - ASP.NET Core MVC med controllere i mappen WebApplication1/Controllers
  - Razor Views i WebApplication1/Views
  - Statisk front-end (CSS, JavaScript, bilder) i WebApplication1/wwwroot

### Domene- og forretningslag:
  - Domeneentiteter i WebApplication1/Models/Entities. Eksempel: UserEntity, Pilot, Registrar, Organization, ObstacleData, ReportItem
  - View-modeller i WebApplication1/Models for å tilpasse data til konkrete views. Eksempel: LoginViewModel, UserProfileViewModel, ReportsIndexViewModel, ValidatedObstacleData
  - Valideringslogikk og forretningsregler implementert i disse modellene og i controllerne

### Datatilgangslag:
  - ApplicationDbContext i WebApplication1/DataInfrastructure/ApplicationDbContext.cs
  - Bruker Entity Framework Core (Pomelo MySQL-provider) for å mappe C#-klasser til tabeller i MariaDB
  - Håndterer relasjoner mellom brukere, piloter, registerførere, organisasjoner, hindere og rapporter

Infrastruktur:
  - docker-compose.yml i rotmappen definerer:
    - db (MariaDB-database)
    - webapplication1 (ASP.NET Core webapplikasjonen)
    - mailhog (lokal e-postserver for utvikling, brukt til å teste «glemt passord» uten å sende ekte e-poster)
  - WebApplication1/Dockerfile beskriver hvordan webapplikasjonen bygges som Docker-image
  - .env.example (mal for miljøvariabler) og .env (lokal konfigurasjon) brukes til å styre database-navn, brukere og passord

Så kort forklart:
- Presentasjonslag: håndterer alt brukerinteraksjon (views, skjema, kart).
- Domene- og forretningslag: inneholder begreper som “hinder”, “rapport”, “pilot”, “registrarfører” og reglene som gjelder for disse.
- Datatilgangslag: kobler domenet til databasen og sørger for lesing/skriving av data på en strukturert måte.
- Infrastruktur: Docker og miljøvariabler gjør at vi kan kjøre hele systemet med få kommandoer og uten manuell databaseoppsett.

**Innlogging, roller og glemt passord**
Innlogging og utlogging håndteres av AccountController. Brukere kan ha ulike roller (for eksempel pilot og registerfører) som styrer hva de har tilgang til i systemet.
Vi har i tillegg implementert en «Glemt passord» / «Forgot password»-funksjon på innloggingssiden, slik man kjenner fra vanlige nettsider. Hvis en bruker har glemt passordet sitt, kan vedkommende:
1. klikke på «Forgot password» på login-siden
2. skrive inn e-postadressen sin
3. motta en engangslenke via e-post (fanget opp av Mailhog i utvikling, ikke sendt «på ordentlig»)
4. klikke på lenken og sette et nytt passord

Selve lenken er tidsbegrenset og kan bare brukes én gang, og systemet lagrer kun en hash av tokenet i databasen. Dette er en trygg og mer realistisk måte å håndtere glemte passord på enn å endre passord manuelt i databasen, samtidig som vi slipper å sende ekte e-poster i utviklingsmiljøet.

## Viktig mappestruktur i repoet

### Rot:
- docker-compose.yml – definisjon av database og webapplikasjon i Docker
- .env.example – mal for miljøvariabler til databasen
- .gitignore – sørger for at sensitive filer (som .env) og genererte filer (bin, obj, opplastede bilder) ikke sjekkes inn
- docs/ – tester og dokumentasjon:

- WebApplication1.sln – Visual Studio / dotnet-løsning

- WebApplication1.Tests:
  - Eget testprosjekt for enhetstesting. Inneholder blant annet ObstacleDataTests.cs som tester validering på hindermodellen

### WebApplication1:
- Program.cs – konfigurerer:
  - logging
  - databasekobling (EF Core med MariaDB)
  - autentisering og autorisasjon (cookies, roller)
  - routing, statiske filer, HTTPS/HSTS

- DataInfrastructure/ApplicationDbContext.cs – EF Core DbContext

- Controllers – controllere for hovedfunksjonene:
- HomeController
- AccountController (innlogging/utlogging og glemt passord)
- UserController (brukerhåndtering og registerførerdashboard)
- ObstacleController (innmelding av hindere via kart)
- ReportsController (liste, detaljer og statusendring for rapporter)

- Models – domene- og view-modeller (blant annet Entities-mappen)

- Views – Razor-views for pilot, registerfører, login osv.

- wwwroot – statiske filer:
  - CSS (for eksempel shared.css, obstacleForm.css, report.css)
  - JavaScript (for eksempel obstacleForm.js for kartlogikk)
  - bilder (inkludert opplastede hinderbilder, som er ekskludert fra Git)

 ---

### Kort testguide for sensor/veileder

1. Start systemet: Klon repoet

2. Kopier .env.dev til .env og fyll inn databaseparametere

3. Kjør docker compose up --build

4. Gå til http://localhost:8080

4. Opprett bruker: Opprett minst én pilot og én registerfører

4. Test pilotflyt:
 - Lag bruker som pilot
 - Log inn
 - Gå til Obstacle registration
 - Registrer et hinder via kartet og send inn
 - Prøv flere hindertyper
 - Legg til bilder
 - Prøv å sende draft uten alle detaljer
 - Trykk på "Reports" og se oversikten av alle rapportene dine
 - Trykk på "Edit" og legg til flere detaljer på draften du lagde og submit

5. Test registerførerflyt:
- Lag bruker som registerfører
- Log inn
- Gå til "Reports" for å se alle rapporter sendt inn
- Åpne en rapport ved å trykke på "Review"
- Endre status, legg til kommentar og lagre
- Sjekk at statusen oppdateres både på dashboard og i rapportoversikten

6. Logg gjerne inn igjen som pilot for å se endringene

7. Test autorisering:
- Prøv å logge inn uten å lage en bruker først
- Prøv å sende inn rapport som registerfører
- Prøv å endre på en rapport som er sendt inn som pilot

8. Test glemt passord (Forgot password):

Sørg for at det finnes en bruker i databasen med en kjent e-postadresse
1. Gå til innloggingssiden og klikk på «Forgot password»
2. Skriv inn e-postadressen til brukeren og send inn
3. Åpne Mailhog i nettleser: http://localhost:8025
4. Finn e-posten som ble sendt, åpne den og klikk på lenken for å resette passord
5. Sett et nytt passord på siden som åpnes og lagre
6. Prøv deretter å logge inn igjen med det nye passordet

## Brukertesting – Sammendrag

Under EXPO gjennomførte vi brukertester av systemet.  
Totalt **5 brukere fikk fullført hele testen**, og **2 brukere fikk påbegynt testen**, men ble avbrutt på grunn av jurybesøk.

Her er hovedfunnene, i uspesifisert rekkefølge:

### Passord og brukeropplevelse
- Brukerne ønsket en **“show password”**-knapp.
- Passordet bør ikke slettes hvis valideringen feiler.
- Behov for tydeligere krav (regex) til passordstyrke.

### Registerførerens arbeidsflate
- Ønske om en **"Behandlet"-tag** for ferdig vurderte rapporter.
- Ikke behov for slettefunksjon – historikk ønskes bevart.
- Ønske om å se hvilken **organisasjon** en bruker tilhører.

### Tilpasning og responsivitet
- Skjermbildet bør skaleres bedre mellom laptop og tablet.

### Kart og obstacle-registrering
- Brukerne ønsket tydeligere forskjell mellom:
  - Pin som markerer valgt posisjon  
  - Pin som viser brukerens egen posisjon  
- Ønske om **default point** for raske registreringer.
- Høyde-feltet bør ha høyere maksverdi.
- Popup ved manglende input etterlyst.
- Bedre håndtering av draft (inkl. redirect).

### Oppsummering
Brukerne klarte å gjennomføre oppgavene, og opplevde systemet som nyttig, men ønsket forbedringer i tydelighet, validering og flyt.
