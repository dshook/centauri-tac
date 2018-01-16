// syncs google sheet to local cards
// NOTE: there seems to be a bug with the sheets library that stops it reading rows when it gets to a blank row

var promise           = require('bluebird');
var fs                = promise.promisifyAll(require('fs'));
var creds             = require('./stac-card-sync.json');
var GoogleSpreadsheet = require("google-sheets-node-api");

const minSyncRow = 182;
const maxSyncRow = 307;

const rowOffset = -2; //I think because starting from row 0 plus the header row?
const cardSet = 'basic';
const directoryPath = __dirname + '/../cards/' + cardSet;
const setIdPrefix = 1000;

async function run(){
    var cards = await loadCards();

    var mySheet = new GoogleSpreadsheet('1bp_H1VVDDzAd5J4rktRTFm88r8h9n7LT0uxg5wGggY4');
    await mySheet.useServiceAccountAuth(creds);

    var sheetInfo = await mySheet.getSpreadsheet();

    var cardList = sheetInfo.worksheets.find(w => w.title === 'cardlist');
    var rows = await cardList.getRows({});

    console.log(rows.length, rows[rows.length - 1].description);

    var aRow = rows[minSyncRow + rowOffset];
    console.log('Row', aRow);

    //not going to work since it's an obj...
    var foundKey = null;
    for (const key of Object.keys(cards)) {
        if(cards[key].cardTemplateId === minSyncRow + setIdPrefix){
            foundKey = key;
            break;
        }
    }
    var foundCard = foundKey ? cards[foundKey] : null;
    console.log('Found Card', foundCard);

    if(foundCard){
        foundCard.name = aRow.name;
        foundCard.movement = aRow.move;

        await fs.writeFileAsync(directoryPath + "/" + foundKey, JSON.stringify(foundCard, null, 2))
    }
}

async function loadCards(){
    let cardRequires = {};
    fs.readdirSync(directoryPath).map(function (filename) {
        let contents = fs.readFileSync(directoryPath + "/" + filename, "utf8");
        try{
            //remove some whitespace before json parsing because json is a stupid format sometimes
            cardRequires[filename] = JSON.parse(contents.replace(/[\t\r\n]/g, ''));
        }catch(e){
            this.log.error('Error loading card ' + filename, e, e.stack);
        }
    });

    return cardRequires;
}

run();