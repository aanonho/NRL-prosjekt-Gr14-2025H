# NRL-prosjekt-Gr14-2025H  
Semesterprosjekt for Kartverket og Norsk Luftambulanse, høst 2025 (gruppe 14).

Dette repoet inneholder en ASP.NET Core MVC-applikasjon for å registrere, håndtere og kvalitetssikre luftfartshindre.  
Piloter kan melde inn hindere via et kart, og registerførere kan se, vurdere og godkjenne/avvise innmeldingene.  
Applikasjon og database kjører sammen i Docker, med MariaDB som database.

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

**Hvorfor har vi gjort det slik?** På grunn av - Sikkerhet: Passord og andre hemmelige ting ligger ikke i kildekode eller docker-compose.yml, og blir ikke lagt ut på GitHub. De finnes nå kun i lokale .env-filer.
- Det er god praksis: Bruk av miljøvariabler og .env er standard praksis for webapplikasjoner og Docker. Dette gjør det enklere å kjøre samme kode i ulike miljøer.
Fleksibilitet
- Hver utvikler (eller sensor) kan bruke egne lokale passord/brukere uten å endre kode eller få passord.
- Enklere vedlikehold: Endringer i databasekonfigurasjon gjøres i én fil (.env), i stedet for i flere ulike configfiler (docker-compose.yml, appsettings*.json, osv.).
 
 Dette er kanskje ikke nødvendig i et lite skoleprosjekt, vi ville vise at vi tenker på sikkerhet og vet hva som er god praksis.

### Kom i gang - Steg 2 Start systemet (web + database)

Fra rotmappen (der docker-compose.yml ligger):
docker compose up --build

**Hva som skjer da:**
- Starter en MariaDB-databasecontainer (db) med brukere/passord fra .env.
- Starter webapplikasjonen (WebApplication1) og kobler den mot databasen via connection string som leses fra ConnectionStrings__DefaultConnection (som igjen er basert på miljøvariabler og MYSQL_*).
- Oppretter en navngitt volume for database-data: nrl-db-data.

Når alt er oppe, er applikasjonen tilgjengelig på:

**http://localhost:8080**
