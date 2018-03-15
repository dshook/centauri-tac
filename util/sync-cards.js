// syncs google sheet to local cards
// NOTE: there seems to be a bug with the sheets library that stops it reading rows when it gets to a blank row

var promise = require('bluebird');
var fs = promise.promisifyAll(require('fs'));
var creds = require('./stac-card-sync.json');
var GoogleSpreadsheet = require("google-sheets-node-api");

//These should obviously mirror what's in the files, just don't want to deal with the import problem or transpilation right now
var races = {
  Neutral: 0,
  Vae: 1,
  Earthlings: 2,
  Martians: 3,
  Grex: 4,
  Phaenon: 5,
  Lost: 6,
};
var rarities = {
  Free: 0,
  Common: 1,
  Rare: 2,
  Exotic: 3,
  Ascendant: 4,
};

const minSyncRow = 68;
const maxSyncRow = 99;

const rowOffset = -2; //I think because starting from row 0 plus the header row?
const cardSet = 'basic';
const directoryPath = __dirname + '/../cards/' + cardSet;

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
    let cardTemplateId = parseInt(sheetRow.id);


    var foundKey = null;
    for (const key of Object.keys(cards)) {
      let cti = getVal(cards[key], 'cardTemplateId');
      if (cti === cardTemplateId) {
        foundKey = key;
        break;
      }
    }
    var foundCard = foundKey ? cards[foundKey] : null;
    console.log('Found Card ' + foundKey);

    let updateData = {
      "name": sheetRow.name,
      "description": sheetRow.description,
      "cost": +sheetRow.cost,
      "attack": +sheetRow.attack,
      "health": +sheetRow.health,
      "movement": +sheetRow.move,
      "range": sheetRow.range == '' ? null : +sheetRow.range,
      "spellDamage": sheetRow.spdmg == '' ? null : +sheetRow.spdmg,
      "race": races[sheetRow.race] !== undefined ? races[sheetRow.race] : null,
      "rarity": rarities[sheetRow.rarity] !== undefined ? rarities[sheetRow.rarity] : null,
      "tags": [sheetRow.cardtype, sheetRow.tribe].filter(t => t)
    };
    if (foundCard) {
      foundCard = replaceVal(foundCard, updateData);

      await fs.writeFileAsync(directoryPath + "/" + foundKey, foundCard)
    } else{
      var newCard = {cardTemplateId: cardTemplateId};
      //map over props but skip null value ones
      for (const key in updateData){
        const element = updateData[key];
        if(element === null){ continue; }
        newCard[key] = element;
      }
      await fs.writeFileAsync(directoryPath + "/" + cardTemplateId + ".json", JSON.stringify(newCard, null, 2))
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
    let updatedLine = `  "${propName}": ${JSON.stringify(value)}`;

    //check for the stupid trailing comma
    if(propIndex > -1 && (!lines[propIndex + 1] || !lines[propIndex + 1].match(/^\}$/gi))){
      updatedLine += ',';
    }

    if (propIndex > -1) {
      if(lines[propIndex].endsWith('[') || lines[propIndex].endsWith('[\r') ){
        //check for multiline arrays written out by json formatter and splice it out
        let endBracketIndex = lines.findIndex((val, idx) =>
          idx > propIndex && (val.endsWith(']') || val.endsWith('],') || val.endsWith(']\r') || val.endsWith('],\r'))
        );
        //check for the stupid trailing comma again...
        if(lines[endBracketIndex + 1].match(/^\}$/gi)){
          updatedLine = updatedLine.substring(0, updatedLine.length - 1);
        }

        lines.splice(propIndex, endBracketIndex - propIndex + 1, updatedLine);
      } if (value === null) {
        //if value is null we shouldn't have this prop at all so remove it from the lines
        lines.splice(propIndex, 1);
      } else {
        lines[propIndex] = updatedLine;
      }
    }else if(value !== null){
      // In this case the prop isn't in the file and we need to stick it somewhere
      // Going to arbitrarily choose after line 9 because it should be after the basic props but before the eventcode
      lines.splice(9, 0, updatedLine);

      if(!lines[8].endsWith(',')){
        lines[8] += ',';
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