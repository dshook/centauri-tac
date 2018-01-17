// syncs google sheet to local cards
// NOTE: there seems to be a bug with the sheets library that stops it reading rows when it gets to a blank row

var promise = require('bluebird');
var fs = promise.promisifyAll(require('fs'));
var creds = require('./stac-card-sync.json');
var GoogleSpreadsheet = require("google-sheets-node-api");

const minSyncRow = 186;
const maxSyncRow = 187;

const rowOffset = -2; //I think because starting from row 0 plus the header row?
const cardSet = 'basic';
const directoryPath = __dirname + '/../cards/' + cardSet;
const setIdPrefix = 1000;

async function run() {
  var cards = await loadCards();

  var mySheet = new GoogleSpreadsheet('1bp_H1VVDDzAd5J4rktRTFm88r8h9n7LT0uxg5wGggY4');
  await mySheet.useServiceAccountAuth(creds);

  var sheetInfo = await mySheet.getSpreadsheet();

  var cardList = sheetInfo.worksheets.find(w => w.title === 'cardlist');
  var rows = await cardList.getRows({});

  console.log(`Fetched ${rows.length} rows`);

  for (let i = minSyncRow; i <= maxSyncRow; i++) {
    var sheetRow = rows[i + rowOffset];

    var foundKey = null;
    for (const key of Object.keys(cards)) {
      let cti = getVal(cards[key], 'cardTemplateId');
      if (cti === i + setIdPrefix) {
        foundKey = key;
        break;
      }
    }
    var foundCard = foundKey ? cards[foundKey] : null;
    console.log('Found Card ' + foundKey);

    if (foundCard) {
      foundCard = replaceVal(foundCard, {
        "name": sheetRow.name,
        "cost": +sheetRow.cost,
        "attack": +sheetRow.attack,
        "health": +sheetRow.health,
        "movement": +sheetRow.move,
        "range": sheetRow.range == '' ? null : +sheetRow.range,
        "spellDamage": sheetRow.spdmg == '' ? null : +sheetRow.spdmg,
      });

      await fs.writeFileAsync(directoryPath + "/" + foundKey, foundCard)
    }
  }
}

async function loadCards() {
  let cardRequires = {};
  fs.readdirSync(directoryPath).map(function (filename) {
    let contents = fs.readFileSync(directoryPath + "/" + filename, "utf8");
    try {
      //Don't json parse the file so the eventcode doesn't get screwed up with its newlines because JSON can't hang
      cardRequires[filename] = contents;
    } catch (e) {
      this.log.error('Error loading card ' + filename, e, e.stack);
    }
  });

  return cardRequires;
}

//String replaces key value pairs inside a JSON like string
function replaceVal(contents, keyValues) {
  let lines = contents.split('\n') || [];
  for (const propName of Object.keys(keyValues)) {
    let value = keyValues[propName];
    let propIndex = lines.findIndex(l => l.trim().startsWith(`"${propName}"`));

    if (propIndex > -1) {
      if (value === null) {
        //if value is null we shouldn't have this prop at all so remove it from the lines
        lines.splice(propIndex, 1);
      } else {
        lines[propIndex] = `  "${propName}": ${JSON.stringify(value)},`;
      }
    }
  }
  return lines.join('\n');
}

//Try to get value out of a JSON like string, only works for single line values lol
function getVal(contents, propName) {
  let lines = contents.split('\n');
  let propIndex = lines.findIndex(l => l.trim().startsWith(`"${propName}"`));
  if (propIndex > -1) {
    //NAS-T hacks
    var bastardizedContent = lines[propIndex]
      .trim()
      .replace('"' + propName + '":', '')
      .trim()
      .replace(/^:/, '')
      .replace(/,$/, '')
      .trim();
    try {
      return JSON.parse(bastardizedContent);
    } catch (e) { }
  }
  return null;
}

run();