// ==UserScript==
// @name         Yu-Gi-Oh! Card Database to YDK(e)
// @namespace    https://github.com/pixeltris/YgoMaster
// @version      1.1
// @description  Adds YDK / YDKe buttons to the official "Yu-Gi-Oh! Card Database" website
// @author       pixeltris
// @match        *://*.db.yugioh-card.com/yugiohdb/member_deck.action*
// @downloadURL  https://github.com/pixeltris/YgoMaster/raw/master/YgoYdk.user.js
// @updateURL    https://github.com/pixeltris/YgoMaster/raw/master/YgoYdk.user.js
// @run-at       document-idle
// @grant        none
// ==/UserScript==
(function() {
    var allCards = [];
    var cidToYdkId = [];
    var loadStep = 0;
    
    function saveFile(fileName, textData) {
        var blob = new Blob([textData], {type: "text/plain;charset=utf-8"});
        if (window.navigator.msSaveOrOpenBlob) {
            window.navigator.msSaveBlob(blob, fileName);
        }
        else {
            var elem = window.document.createElement('a');
            elem.href = window.URL.createObjectURL(blob);
            elem.download = fileName;
            document.body.appendChild(elem);
            elem.click();
            document.body.removeChild(elem);
        }
    }
    
    var targetElement = document.getElementsByClassName('sort_set');
    if (targetElement.length == 1) {
        var newDiv = document.createElement('div');
        newDiv.style.textAlign = 'right';
        
        var missingCardsLabel = document.createElement('p');
        missingCardsLabel.innerHTML = '';
        missingCardsLabel.style.display = 'inline';
        missingCardsLabel.style.paddingRight = '5px';
        newDiv.appendChild(missingCardsLabel);
        
        var ydkBtn = document.createElement('button');
        ydkBtn.innerHTML = 'Save YDK';
        ydkBtn.onclick = function() {
            var cardList = getCardList();
            var ydk = '#main\n';
            for (var i = 0; i < cardList.main.length; i++) {
                ydk += cidToYdkId[cardList.main[i]] + '\n';
            }
            ydk += '#extra\n';
            for (var i = 0; i < cardList.extra.length; i++) {
                ydk += cidToYdkId[cardList.extra[i]] + '\n';
            }
            ydk += '!side\n';
            for (var i = 0; i < cardList.side.length; i++) {
                ydk += cidToYdkId[cardList.side[i]] + '\n';
            }
            var ydkName = new URL(window.location.href).searchParams.get('cgid') + '.ydk';
            saveFile(ydkName, ydk);
        };
        newDiv.appendChild(ydkBtn);
        
        var ydkeBtn = document.createElement('button');
        ydkeBtn.innerHTML = 'Copy YDKe';
        ydkeBtn.onclick = function() {
            var cardList = getCardList();
            var main = [];
            for (var i = 0; i < cardList.main.length; i++) {
                main.push(cidToYdkId[cardList.main[i]] | 0);
            }
            var extra = [];
            for (var i = 0; i < cardList.extra.length; i++) {
                extra.push(cidToYdkId[cardList.extra[i]] | 0);
            }
            var side = [];
            for (var i = 0; i < cardList.side.length; i++) {
                side.push(cidToYdkId[cardList.side[i]] | 0);
            }
            var ydke = 'ydke://' +
                btoa(String.fromCharCode.apply(null, new Uint8Array(new Uint32Array(main).buffer))) + '!' +
                btoa(String.fromCharCode.apply(null, new Uint8Array(new Uint32Array(extra).buffer))) + '!' +
                btoa(String.fromCharCode.apply(null, new Uint8Array(new Uint32Array(side).buffer))) + '!';
            navigator.clipboard.writeText(ydke);
        };
        newDiv.appendChild(ydkeBtn);
        
        targetElement[0].parentNode.parentNode.insertBefore(newDiv, targetElement[0].parentNode.nextSibling);
        
        function updateCards() {
            var numMissingCards = 0;
            var cardList = getCardList();
            for (var i = 0; i < cardList.all.length; i++) {
                if (!allCards[cardList.all[i]] || !cidToYdkId[cardList.all[i]]) {
                    numMissingCards++;
                }
            }
            if (numMissingCards > 0) {
                missingCardsLabel.innerHTML = numMissingCards + ' missing card(s)';
            }
        }
        function onLoadedAllCards(cards) {
            allCards = cards;
            if (++loadStep == 2) {
                updateCards();
            }
        }
        function onLoadedYdkIds(data) {
            var splitted = data.split(/\r?\n/);
            for (var i = 0; i < splitted.length; i++) {
                var splitted2 = splitted[i].split(' ');
                if (splitted2.length >= 2 && splitted2[0] && splitted2[1] && !cidToYdkId[splitted2[1]]) {
                    cidToYdkId[splitted2[1]] = splitted2[0];
                }
            }
            if (++loadStep == 2) {
                updateCards();
            }
        }
        fetch('https://raw.githubusercontent.com/pixeltris/YgoMaster/master/Build/Data/CardList.json')
            .then(response => response.json())
            .then(data => onLoadedAllCards(data));
        fetch('https://raw.githubusercontent.com/pixeltris/YgoMaster/master/Build/Data/YdkIds.txt')
            .then(response => response.text())
            .then(data => onLoadedYdkIds(data));
    }
    function getCardList() {
        var result = {
            main: [],
            extra: [],
            side: [],
            all: []
        };
        var deckData = document.getElementById('deck_image');
        var imageTables = deckData.getElementsByClassName('image_set');
        for (var i = 0; i < 3 && i < imageTables.length; i++) {
            var cardList = null;
            switch (i) {
                case 0: cardList = result.main; break;
                case 1: cardList = result.extra; break;
                case 2: cardList = result.side; break;
            }
            var allLinks = imageTables[i].querySelectorAll('a');
            for (var j = 0; j < allLinks.length; j++) {
                var cid = new URL(allLinks[j].href).searchParams.get('cid');
                if (cid) {
                    if (allCards[cid]) {
                        cardList.push(cid);
                    }
                    result.all.push(cid);
                }
            }
        }
        return result;
    }
})();