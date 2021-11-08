
    // JavaScript Document
    var inPracticeBlock = false;
    var currentItemKeyedDir = "Left";
    var currentContinueKeyCode = 32;
    var isErrorMarked = false;
    var currentHandler = null;
    var currentStimulus = null;
    var currentItemNum = 0;
    var currentItemID;
    var EventList = new Array();
    var EventCtr = 0;
    var ErrorMark;

    var CookieUtil = {
    getCookie : function(name) {
    var cookieName = encodeURIComponent(name) + "=";
    var cookieStart = document.cookie.indexOf(cookieName);
    var cookieValue = null;

    if (cookieStart > -1) {
    var cookieEnd = document.cookie.indexOf(";", cookieStart)
    if (cookieEnd == -1)
    cookieEnd = document.cookie.length;
    cookieValue = decodeURIComponent(document.cookie.substring(cookieStart + cookieName.length, cookieEnd));
    }
    }};

    var EventUtil = {
    addHandler: function(element, type, handler) {
    if (element.addEventListner) {
    element.addEventListener(type, handler, false);
    } else if (element.attachEvent) {
    element.attachEvent("on" + type, handler);
    } else {
    element["on" + type] = handler;
    }
    },

    getEvent: function(event) {
    return event ? event : window.event;
    },

    getTarget: function(event) {
    return event.target || event.srcElement;
    },

    preventDefault: function(event) {
    if (event.preventDefault) {
    event.preventDefault();
    } else {
    event.returnValue = false;
    }
    },

    removeHandler: function(element, type, handler) {
    if (element.removeEventListener) {
    element.removeEventListener(type, handler, false);
    }
    else if (element.detachEvent) {
    element.detachEvent("on" + type, handler);
    } else {
    element["on" + type] = null;
    }
    },

    stopPropagation: function(event) {
    if (event.stopPropagation) {
    event.stopPropagation();
    } else {
    event.cancelBubble = true;
    }
    },

    getCharCode: function(event) {
    if (typeof event.charCode == "number") {
    return event.charCode;
    } else {
    return event.keyCode;
    }
    }
    };

    /// IATDisplayItem
    function IATDisplayItem(id, src, x, y, width, height)
    {
    this.id = id;
    this.src = "http://localhost:8080/IATServer/sexualattitudes/"+ src;
    this.x = x;
    this.y = y;
    this.width = width;
    this.height = height;
    this.imgTag = document.createElement("img");
    this.imgTag.id = "IATDisplayItem" + id.toString();
    }

    IATDisplayItem.prototype = {
    constructor: IATDisplayItem,
    Load : function() {
    this.img = new Image();
    this.img.src = this.src;
    },

    Outline : function()
    {
    this.imgTag.className = "outlinedDI";
    },

    Display : function(parentNode) {
    this.imgTag.src = this.src;
    parentNode.appendChild(this.imgTag);
    },

    Hide : function() {
    if (this.imgTag.parentNode) {
    this.imgTag.parentNode.removeChild(this.imgTag);
    }
    this.imgTag.className = "";
    }
    };

    /// IATLayout
    function IATDisplay(interiorWidth, interiorHeight, leftResponseKeyCodeUpper, leftResponseKeyCodeLower, rightResponseKeyCodeUpper, rightResponseKeyCodeLower)
    {
    this.interiorWidth = interiorWidth;
    this.interiorHeight = interiorHeight;
    this.leftResponseKeyCodeUpper = leftResponseKeyCodeUpper;
    this.leftResponseKeyCodeLower = leftResponseKeyCodeLower;
    this.rightResponseKeyCodeUpper = rightResponseKeyCodeUpper;
    this.rightResponseKeyCodeLower = rightResponseKeyCodeLower;
    this.startTime = -1;

    this.divTag = document.getElementById("IATDisplayDiv");

    this.displayItems = new Array();
    }

    IATDisplay.prototype = {
    constructor : IATDisplay,
    AddDisplayItem : function(di) {
    this.displayItems[this.displayItems.length] = di;
    di.Display(this.divTag);
    },

    RemoveDisplayItem : function(di) {
    for (var ctr = 0; ctr < this.displayItems.length; ctr++)
    {
    if (this.displayItems[ctr].id == di.id)
    {
    this.displayItems[ctr].Hide();
    this.displayItems.splice(ctr, 1);
    }
    }
    },

    Clear : function() {
    for (var ctr = 0; ctr < this.displayItems.length; ctr++)
    this.displayItems[ctr].Hide();
    this.displayItems.splice(0, this.displayItems.length);
    },

    StartTimer : function() {
    this.startTime = (new Date()).getTime();
    }
    };

    function IATEvent(id, handler)
    {
    this.id = id;
    this.handler = handler;
    }

    IATEvent.prototype = {
    constructor : IATEvent,
    Execute : function() {
    if (this.handler == null)
    EventList[++EventCtr].Execute();
    else {
    currentHandler = this.handler;
    EventUtil.addHandler(Display.divTag, "keypress", this.handler);
    }
    }
    };

    function IATSubmitEvent(id)
    {
    IATEvent.call(this, id, null);
    }

    IATSubmitEvent.prototype = {
    constructor : IATSubmitEvent,
    Execute : function() {
    var form = document.getElementById("IATForm");
    form.submit();
    }};


    function IATItem(id, stimulus, itemNum, keyedDir)
    {
    if (keyedDir == "Left")
    {
    IATEvent.call(this, id, function(event)
    {
    var keyCode = EventUtil.getCharCode(event);
    if ((keyCode == Display.leftResponseKeyCodeUpper) || (keyCode == Display.leftResponseKeyCodeLower))
    {
    if (!inPracticeBlock)
    {
    var latencyItemName = docuement.createElement("input");
    latencyItemName.name = "ItemNum" + currentItemNum.toString();
    latencyItemName.value = currentItemID.toString();
    latencyItemName.type = "hidden";
    Display.divTag.appendChild(latencyItemName);
    var latency = (new Date()).getTime() - Display.startTime;
    var latencyOutput = document.createElement("input");
    latencyOutput.name = "Item" + currentItemNum.toString();
    currentItemNum++;
    latencyOutput.type = "hidden";
    latencyOutput.value = latency.toString();
    Display.divTag.appendChild(latencyOutput);
    }
    if (isErrorMarked)
    Display.RemoveDisplayItem(ErrorMark);
    isErrorMarked = false;
    Display.RemoveDisplayItem(currentStimulus);
    EventUtil.removeHandler(Display.divTag, "keypress", currentHandler);
    EventList[++EventCtr].Execute();
    }
    else if ((keyCode == Display.rightResponseKeyCodeUpper) || (keyCode == Display.rightResponseKeyCodeLower))
    {
    if (!isErrorMarked) {
    Display.AddDisplayItem(ErrorMark);
    isErrorMarked = true;
    }
    }
    });
    }
    else {
    IATEvent.call(this, id, function(event)
    {
    var keyCode = EventUtil.getCharCode(event);
    if ((keyCode == Display.rightResponseKeyCodeUpper) || (keyCode == Display.rightResponseKeyCodeLower))
    {
    if (!inPracticeBlock)
    {
    var latencyItemName = docuement.createElement("input");
    latencyItemName.name = "ItemNum" + currentItemNum.toString();
    latencyItemName.value = currentItemID.toString();
    latencyItemName.type = "hidden";
    Display.divTag.appendChild(latencyItemName);
    var latency = (new Date()).getTime() - Display.startTime;
    var latencyOutput = document.createElement("input");
    latencyOutput.name = "Item" + currentItemNum.toString();
    currentItemNum++;
    latencyOutput.type = "hidden";
    latencyOutput.value = latency.toString();
    Display.divTag.appendChild(latencyOutput);
    }
    if (isErrorMarked)
    Display.RemoveDisplayItem(ErrorMark);
    Display.RemoveDisplayItem(currentStimulus);
    EventUtil.removeHandler(Display.divTag, "keypress", currentHandler);
    EventList[++EventCtr].Execute();
    }
    else if ((keyCode == Display.leftResponseKeyCodeUpper) || (keyCode == Display.leftResponseKeyCodeLower))
    {
    if (!isErrorMarked) {
    Display.AddDisplayItem(ErrorMark);
    isErrorMarked = true;
    }
    }

    });
    }
    this.isErrorMarked = false;
    this.stimulus = stimulus;
    this.itemNum = itemNum;
    }

    IATItem.prototype = {
    constructor : IATItem,
    Execute : function() {
    currentItemKeyedDir = this.keyedDir;
    currentStimulus = this.stimulus;
    currentItemID = this.itemNum;
    Display.AddDisplayItem(this.stimulus);
    Display.StartTimer();
    IATEvent.prototype.Execute.call(this);
    }
    };

    function IATBeginBlock(id, isPracticeBlock, leftDisplayItem, rightDisplayItem, instructionsDisplayItem)
    {
    IATEvent.call(this, id, null);
    this.isPracticeBlock = isPracticeBlock;
    this.leftDisplayItem = leftDisplayItem;
    this.rightDisplayItem = rightDisplayItem;
    this.instructionsDisplayItem = instructionsDisplayItem;
    }

    IATBeginBlock.prototype = {
    constructor : IATBeginBlock,
    Execute : function() {
    inPracticeBlock = this.isPracticeBlock;
    Display.AddDisplayItem(this.leftDisplayItem);
    Display.AddDisplayItem(this.rightDisplayItem);
    Display.AddDisplayItem(this.instructionsDisplayItem);
    IATEvent.prototype.Execute.call(this);
    }
    };

    function IATEndBlock(id)
    {
    IATEvent.call(this, id, null);
    }

    IATEndBlock.prototype = {
    constructor : IATEndBlock,
    Execute : function() {
    inPracticeBlock = false;
    Display.Clear();
    IATEvent.prototype.Execute.call(this);
    }
    };

    function IATInstructionScreen(id, continueChar, continueInstructionsDI)
    {
    IATEvent.call(this, id, function(event) {
    var keyCode = EventUtil.getCharCode(event);
    if (keyCode == currentContinueKeyCode)
    {
    Display.Clear();
    EventUtil.removeHandler(Display.divTag, "keypress", currentHandler);
    IATEvent.prototype.Execute.call(this);
    }
    });

    this.continueChar = continueChar;
    this.continueInstructionsDI = continueInstructionsDI;
    }

    IATInstructionScreen.prototype =
    {
    constructor : IATInstructionScreen,
    Execute : function()
    {
    Display.AddDisplayItem(this.continueInstructionsDI);
    currentContinueKeyCode = this.continueChar;
    IATEvent.prototype.Execute.call(this);
    }
    };

    function IATTextInstructionScreen(id, continueChar, continueInstructionsDI, textInstructionsDI)
    {
    IATInstructionScreen.call(this, id, continueChar, continueInstructionsDI);
    this.textInstructionsDI = textInstructionsDI;
    }

    IATTextInstructionScreen.prototype = {
    constructor : IATTextInstructionScreen,
    Execute : function()
    {
    Display.AddDisplayItem(this.textInstructionsDI);
    IATInstructionScreen.prototype.Execute.call(this);
    }
    };

    function IATMockItemInstructionScreen(id, continueChar, continueInstructionsDI, leftResponseDI, rightResponseDI, stimulusDI, instructionsDI, errorMarked, outlineLeftResponse, outlineRightResponse)
    {
    IATInstructionScreen.call(this, id, continueChar, continueInstructionsDI);
    this.leftResponseDI = leftResponseDI;
    this.rightResponseDI = rightResponseDI;
    this.stimulusDI = stimulusDI;
    this.instructionsDI = instructionsDI;
    this.errorMarked = errorMarked;
    this.outlineLeftResponse = outlineLeftResponse;
    this.outlineRightResponse = outlineRightResponse;
    }

    IATMockItemInstructionScreen.prototype = {
    constructor : IATMockItemInstructionScreen,
    Execute : function()
    {
    if (this.outlineLeftResponse)
    this.leftResponseDI.Outline();
    Display.AddDisplayItem(this.leftResponseDI);
    if (this.outlineRightResponse)
    this.rightResponseDI.Outline();
    Display.AddDisplayItem(this.rightResponseDI);
    Display.AddDisplayItem(this.stimulusDI);
    Display.AddDisplayItem(this.instructionsDI);
    if (this.errorMarked)
    Display.AddDisplayItem(ErrorMark);
    IATInstructionScreen.prototype.Execute.call(this);
    }
    };

    function IATKeyedInstructionScreen(id, continueChar, continueInstructionsDI, instructionsDI, leftResponseDI, rightResponseDI)
    {
    IATInstructionScreen.call(this, id, continueChar, continueInstructionsDI);
    this.instructionsDI = instructionsDI;
    this.leftResponseDI = leftResponseDI;
    this.rightResponseDI = rightResponseDI;
    }

    IATKeyedInstructionScreen.prototype = {
    constructor : IATKeyedInstructionScreen,
    Execute : function()
    {
    Display.AddDisplayItem(this.instructionsDI);
    Display.AddDisplayItem(this.leftResponseDI);
    Display.AddDisplayItem(this.rightResponseDI);
    IATInstructionScreen.prototype.Execute.call(this);
    }
    };

    function IATBlock(blockNum, numPresentations, alternatedWith)
    {
    this.blockNum = blockNum;
    this.numPresentations = numPresentations;
    this.alternatedWith = alternatedWith;
    this.BeginBlockEvent = null;
    this.EndBlockEvent = null;
    this.Items = new Array();
    }

    IATBlock.prototype = {
    constructor : IATBlock,
    AddItem : function(item)
    {
    this.Items.push(item);
    },

    GenerateContents : function(randomization)
    {
    var result = new Array();
    result.push(this.BeginBlockEvent);
    var ctr;
    if (randomization == "None")
    {
    for (ctr = 0; ctr < Items.length; ctr++)
    result.push(this.Items[ctr]);
    }
    else if (randomization == "RandomOrder")
    {
    var tempItems = new Array();
    for (ctr = 0; ctr < this.Items.length; ctr++)
    tempItems.push(this.Items[ctr]);
    for (ctr = 0; ctr < this.Items.length; ctr++)
    {
    var ndx = Math.floor(Math.random() * tempItems.length);
    result.push(tempItems[ndx]);
    tempItems.splice(ndx, 1);
    }
    } else if (randomization == "SetNumberOfPresentations")
    {
    for (ctr = 0; ctr < Items.length; ctr++)
    result.push(this.Items[Math.floor(Math.random() * Items.length)]);
    }
    result.push(this.EndBlockEvent);
    return result;
    }
    };

    function IATInstructionBlock(alternatedWith)
    {
    this.alternatedWith = alternatedWith;
    this.screens = new Array();
    }

    IATInstructionBlock.prototype = {
    constructor : IATInstructionBlock,
    AddScreen : function(screen) {
    this.screens.push(screen);
    }};

    var Display = new IATDisplay(525, 525, 69, 101, 73, 105);
    
    var LoadingImagesElement = document.createElement("h3");
    var LoadingImagesText = document.createTextNode("Please Wait");
    var b;
    LoadingImagesElement.appendChild(LoadingImagesText);
    var LoadingImagesProgressElement = document.createElement("h4");
    var LoadingImagesProgressText = document.createTextNode("");
    LoadingImagesProgressElement.appendChild(LoadingImagesProgressText);
    Display.divTag.appendChild(LoadingImagesElement);
    Display.divTag.appendChild(LoadingImagesProgressElement);
    
      LoadingImagesProgressText.nodeValue = "Loading image 1 of 82";
      var DisplayItem0 = new IATDisplayItem(0, "sexualattitudes0000.png", 25, 25, 475, 445);
      DisplayItem0.Load();
      ///    while (DisplayItem0.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 2 of 82";
      var DisplayItem1 = new IATDisplayItem(1, "sexualattitudes0001.png", 144, 506, 237, 19);
      DisplayItem1.Load();
      ///    while (DisplayItem1.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 3 of 82";
      var DisplayItem2 = new IATDisplayItem(2, "sexualattitudes0002.png", 56, 50, 47, 19);
      DisplayItem2.Load();
      ///    while (DisplayItem2.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 4 of 82";
      var DisplayItem3 = new IATDisplayItem(3, "sexualattitudes0003.png", 427, 50, 35, 19);
      DisplayItem3.Load();
      ///    while (DisplayItem3.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 5 of 82";
      var DisplayItem4 = new IATDisplayItem(4, "sexualattitudes0004.png", 235, 177, 54, 19);
      DisplayItem4.Load();
      ///    while (DisplayItem4.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 6 of 82";
      var DisplayItem5 = new IATDisplayItem(5, "sexualattitudes0005.png", 27, 425, 470, 70);
      DisplayItem5.Load();
      ///    while (DisplayItem5.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 7 of 82";
      var DisplayItem6 = new IATDisplayItem(6, "sexualattitudes0006.png", 144, 506, 237, 19);
      DisplayItem6.Load();
      ///    while (DisplayItem6.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 8 of 82";
      var DisplayItem7 = new IATDisplayItem(7, "sexualattitudes0007.png", 25, 135, 475, 335);
      DisplayItem7.Load();
      ///    while (DisplayItem7.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 9 of 82";
      var DisplayItem8 = new IATDisplayItem(8, "sexualattitudes0008.png", 56, 50, 47, 19);
      DisplayItem8.Load();
      ///    while (DisplayItem8.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 10 of 82";
      var DisplayItem9 = new IATDisplayItem(9, "sexualattitudes0009.png", 427, 50, 35, 19);
      DisplayItem9.Load();
      ///    while (DisplayItem9.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 11 of 82";
      var DisplayItem10 = new IATDisplayItem(10, "sexualattitudes0010.png", 144, 506, 237, 19);
      DisplayItem10.Load();
      ///    while (DisplayItem10.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 12 of 82";
      var DisplayItem11 = new IATDisplayItem(11, "sexualattitudes0011.png", 56, 50, 47, 19);
      DisplayItem11.Load();
      ///    while (DisplayItem11.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 13 of 82";
      var DisplayItem12 = new IATDisplayItem(12, "sexualattitudes0012.png", 427, 50, 35, 19);
      DisplayItem12.Load();
      ///    while (DisplayItem12.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 14 of 82";
      var DisplayItem13 = new IATDisplayItem(13, "sexualattitudes0013.png", 27, 425, 470, 100);
      DisplayItem13.Load();
      ///    while (DisplayItem13.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 15 of 82";
      var DisplayItem14 = new IATDisplayItem(14, "sexualattitudes0014.png", 245, 177, 33, 19);
      DisplayItem14.Load();
      ///    while (DisplayItem14.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 16 of 82";
      var DisplayItem15 = new IATDisplayItem(15, "sexualattitudes0015.png", 228, 177, 68, 19);
      DisplayItem15.Load();
      ///    while (DisplayItem15.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 17 of 82";
      var DisplayItem16 = new IATDisplayItem(16, "sexualattitudes0016.png", 241, 177, 42, 19);
      DisplayItem16.Load();
      ///    while (DisplayItem16.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 18 of 82";
      var DisplayItem17 = new IATDisplayItem(17, "sexualattitudes0017.png", 228, 177, 68, 19);
      DisplayItem17.Load();
      ///    while (DisplayItem17.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 19 of 82";
      var DisplayItem18 = new IATDisplayItem(18, "sexualattitudes0018.png", 237, 177, 49, 19);
      DisplayItem18.Load();
      ///    while (DisplayItem18.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 20 of 82";
      var DisplayItem19 = new IATDisplayItem(19, "sexualattitudes0019.png", 221, 177, 82, 19);
      DisplayItem19.Load();
      ///    while (DisplayItem19.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 21 of 82";
      var DisplayItem20 = new IATDisplayItem(20, "sexualattitudes0020.png", 226, 177, 71, 19);
      DisplayItem20.Load();
      ///    while (DisplayItem20.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 22 of 82";
      var DisplayItem21 = new IATDisplayItem(21, "sexualattitudes0021.png", 242, 177, 40, 19);
      DisplayItem21.Load();
      ///    while (DisplayItem21.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 23 of 82";
      var DisplayItem22 = new IATDisplayItem(22, "sexualattitudes0022.png", 230, 177, 64, 19);
      DisplayItem22.Load();
      ///    while (DisplayItem22.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 24 of 82";
      var DisplayItem23 = new IATDisplayItem(23, "sexualattitudes0023.png", 245, 177, 34, 19);
      DisplayItem23.Load();
      ///    while (DisplayItem23.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 25 of 82";
      var DisplayItem24 = new IATDisplayItem(24, "sexualattitudes0024.png", 233, 177, 57, 19);
      DisplayItem24.Load();
      ///    while (DisplayItem24.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 26 of 82";
      var DisplayItem25 = new IATDisplayItem(25, "sexualattitudes0025.png", 229, 177, 66, 19);
      DisplayItem25.Load();
      ///    while (DisplayItem25.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 27 of 82";
      var DisplayItem26 = new IATDisplayItem(26, "sexualattitudes0026.png", 238, 177, 48, 19);
      DisplayItem26.Load();
      ///    while (DisplayItem26.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 28 of 82";
      var DisplayItem27 = new IATDisplayItem(27, "sexualattitudes0027.png", 234, 177, 56, 19);
      DisplayItem27.Load();
      ///    while (DisplayItem27.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 29 of 82";
      var DisplayItem28 = new IATDisplayItem(28, "sexualattitudes0028.png", 241, 177, 42, 19);
      DisplayItem28.Load();
      ///    while (DisplayItem28.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 30 of 82";
      var DisplayItem29 = new IATDisplayItem(29, "sexualattitudes0029.png", 25, 135, 475, 335);
      DisplayItem29.Load();
      ///    while (DisplayItem29.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 31 of 82";
      var DisplayItem30 = new IATDisplayItem(30, "sexualattitudes0030.png", 63, 50, 34, 19);
      DisplayItem30.Load();
      ///    while (DisplayItem30.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 32 of 82";
      var DisplayItem31 = new IATDisplayItem(31, "sexualattitudes0031.png", 412, 50, 66, 19);
      DisplayItem31.Load();
      ///    while (DisplayItem31.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 33 of 82";
      var DisplayItem32 = new IATDisplayItem(32, "sexualattitudes0032.png", 144, 506, 237, 19);
      DisplayItem32.Load();
      ///    while (DisplayItem32.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 34 of 82";
      var DisplayItem33 = new IATDisplayItem(33, "sexualattitudes0033.png", 63, 50, 34, 19);
      DisplayItem33.Load();
      ///    while (DisplayItem33.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 35 of 82";
      var DisplayItem34 = new IATDisplayItem(34, "sexualattitudes0034.png", 412, 50, 66, 19);
      DisplayItem34.Load();
      ///    while (DisplayItem34.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 36 of 82";
      var DisplayItem35 = new IATDisplayItem(35, "sexualattitudes0035.png", 27, 425, 470, 100);
      DisplayItem35.Load();
      ///    while (DisplayItem35.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 37 of 82";
      var DisplayItem36 = new IATDisplayItem(36, "sexualattitudes0036.jpeg", 137, 127, 250, 240);
      DisplayItem36.Load();
      ///    while (DisplayItem36.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 38 of 82";
      var DisplayItem37 = new IATDisplayItem(37, "sexualattitudes0037.jpeg", 99, 127, 325, 240);
      DisplayItem37.Load();
      ///    while (DisplayItem37.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 39 of 82";
      var DisplayItem38 = new IATDisplayItem(38, "sexualattitudes0038.jpeg", 114, 127, 295, 240);
      DisplayItem38.Load();
      ///    while (DisplayItem38.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 40 of 82";
      var DisplayItem39 = new IATDisplayItem(39, "sexualattitudes0039.jpeg", 72, 129, 380, 235);
      DisplayItem39.Load();
      ///    while (DisplayItem39.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 41 of 82";
      var DisplayItem40 = new IATDisplayItem(40, "sexualattitudes0040.jpeg", 186, 127, 152, 240);
      DisplayItem40.Load();
      ///    while (DisplayItem40.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 42 of 82";
      var DisplayItem41 = new IATDisplayItem(41, "sexualattitudes0041.jpeg", 155, 127, 213, 240);
      DisplayItem41.Load();
      ///    while (DisplayItem41.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 43 of 82";
      var DisplayItem42 = new IATDisplayItem(42, "sexualattitudes0042.jpeg", 100, 127, 323, 240);
      DisplayItem42.Load();
      ///    while (DisplayItem42.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 44 of 82";
      var DisplayItem43 = new IATDisplayItem(43, "sexualattitudes0043.jpeg", 154, 127, 216, 240);
      DisplayItem43.Load();
      ///    while (DisplayItem43.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 45 of 82";
      var DisplayItem44 = new IATDisplayItem(44, "sexualattitudes0044.jpeg", 72, 166, 380, 162);
      DisplayItem44.Load();
      ///    while (DisplayItem44.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 46 of 82";
      var DisplayItem45 = new IATDisplayItem(45, "sexualattitudes0045.jpeg", 72, 176, 380, 141);
      DisplayItem45.Load();
      ///    while (DisplayItem45.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 47 of 82";
      var DisplayItem46 = new IATDisplayItem(46, "sexualattitudes0046.jpeg", 127, 127, 270, 240);
      DisplayItem46.Load();
      ///    while (DisplayItem46.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 48 of 82";
      var DisplayItem47 = new IATDisplayItem(47, "sexualattitudes0047.jpeg", 72, 152, 380, 189);
      DisplayItem47.Load();
      ///    while (DisplayItem47.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 49 of 82";
      var DisplayItem48 = new IATDisplayItem(48, "sexualattitudes0048.jpeg", 72, 160, 380, 173);
      DisplayItem48.Load();
      ///    while (DisplayItem48.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 50 of 82";
      var DisplayItem49 = new IATDisplayItem(49, "sexualattitudes0049.jpeg", 72, 138, 380, 218);
      DisplayItem49.Load();
      ///    while (DisplayItem49.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 51 of 82";
      var DisplayItem50 = new IATDisplayItem(50, "sexualattitudes0050.jpeg", 150, 127, 224, 240);
      DisplayItem50.Load();
      ///    while (DisplayItem50.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 52 of 82";
      var DisplayItem51 = new IATDisplayItem(51, "sexualattitudes0051.jpeg", 86, 127, 352, 240);
      DisplayItem51.Load();
      ///    while (DisplayItem51.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 53 of 82";
      var DisplayItem52 = new IATDisplayItem(52, "sexualattitudes0052.png", 25, 135, 475, 335);
      DisplayItem52.Load();
      ///    while (DisplayItem52.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 54 of 82";
      var DisplayItem53 = new IATDisplayItem(53, "sexualattitudes0053.jpeg", 5, 5, 150, 110);
      DisplayItem53.Load();
      ///    while (DisplayItem53.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 55 of 82";
      var DisplayItem54 = new IATDisplayItem(54, "sexualattitudes0054.jpeg", 370, 5, 150, 110);
      DisplayItem54.Load();
      ///    while (DisplayItem54.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 56 of 82";
      var DisplayItem55 = new IATDisplayItem(55, "sexualattitudes0055.png", 144, 506, 237, 19);
      DisplayItem55.Load();
      ///    while (DisplayItem55.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 57 of 82";
      var DisplayItem56 = new IATDisplayItem(56, "sexualattitudes0056.jpeg", 5, 5, 150, 110);
      DisplayItem56.Load();
      ///    while (DisplayItem56.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 58 of 82";
      var DisplayItem57 = new IATDisplayItem(57, "sexualattitudes0057.jpeg", 370, 5, 150, 110);
      DisplayItem57.Load();
      ///    while (DisplayItem57.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 59 of 82";
      var DisplayItem58 = new IATDisplayItem(58, "sexualattitudes0058.png", 27, 425, 470, 100);
      DisplayItem58.Load();
      ///    while (DisplayItem58.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 60 of 82";
      var DisplayItem59 = new IATDisplayItem(59, "sexualattitudes0059.jpeg", 5, 5, 150, 110);
      DisplayItem59.Load();
      ///    while (DisplayItem59.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 61 of 82";
      var DisplayItem60 = new IATDisplayItem(60, "sexualattitudes0060.jpeg", 370, 5, 150, 110);
      DisplayItem60.Load();
      ///    while (DisplayItem60.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 62 of 82";
      var DisplayItem61 = new IATDisplayItem(61, "sexualattitudes0061.png", 27, 425, 470, 100);
      DisplayItem61.Load();
      ///    while (DisplayItem61.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 63 of 82";
      var DisplayItem62 = new IATDisplayItem(62, "sexualattitudes0062.png", 25, 135, 475, 335);
      DisplayItem62.Load();
      ///    while (DisplayItem62.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 64 of 82";
      var DisplayItem63 = new IATDisplayItem(63, "sexualattitudes0063.png", 47, 50, 66, 19);
      DisplayItem63.Load();
      ///    while (DisplayItem63.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 65 of 82";
      var DisplayItem64 = new IATDisplayItem(64, "sexualattitudes0064.png", 428, 50, 34, 19);
      DisplayItem64.Load();
      ///    while (DisplayItem64.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 66 of 82";
      var DisplayItem65 = new IATDisplayItem(65, "sexualattitudes0065.png", 144, 506, 237, 19);
      DisplayItem65.Load();
      ///    while (DisplayItem65.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 67 of 82";
      var DisplayItem66 = new IATDisplayItem(66, "sexualattitudes0066.png", 47, 50, 66, 19);
      DisplayItem66.Load();
      ///    while (DisplayItem66.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 68 of 82";
      var DisplayItem67 = new IATDisplayItem(67, "sexualattitudes0067.png", 428, 50, 34, 19);
      DisplayItem67.Load();
      ///    while (DisplayItem67.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 69 of 82";
      var DisplayItem68 = new IATDisplayItem(68, "sexualattitudes0068.png", 27, 425, 470, 100);
      DisplayItem68.Load();
      ///    while (DisplayItem68.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 70 of 82";
      var DisplayItem69 = new IATDisplayItem(69, "sexualattitudes0069.png", 25, 135, 475, 335);
      DisplayItem69.Load();
      ///    while (DisplayItem69.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 71 of 82";
      var DisplayItem70 = new IATDisplayItem(70, "sexualattitudes0070.jpeg", 5, 5, 150, 110);
      DisplayItem70.Load();
      ///    while (DisplayItem70.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 72 of 82";
      var DisplayItem71 = new IATDisplayItem(71, "sexualattitudes0071.jpeg", 370, 5, 150, 110);
      DisplayItem71.Load();
      ///    while (DisplayItem71.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 73 of 82";
      var DisplayItem72 = new IATDisplayItem(72, "sexualattitudes0072.png", 144, 506, 237, 19);
      DisplayItem72.Load();
      ///    while (DisplayItem72.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 74 of 82";
      var DisplayItem73 = new IATDisplayItem(73, "sexualattitudes0073.jpeg", 5, 5, 150, 110);
      DisplayItem73.Load();
      ///    while (DisplayItem73.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 75 of 82";
      var DisplayItem74 = new IATDisplayItem(74, "sexualattitudes0074.jpeg", 370, 5, 150, 110);
      DisplayItem74.Load();
      ///    while (DisplayItem74.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 76 of 82";
      var DisplayItem75 = new IATDisplayItem(75, "sexualattitudes0075.png", 27, 425, 470, 100);
      DisplayItem75.Load();
      ///    while (DisplayItem75.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 77 of 82";
      var DisplayItem76 = new IATDisplayItem(76, "sexualattitudes0076.jpeg", 5, 5, 150, 110);
      DisplayItem76.Load();
      ///    while (DisplayItem76.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 78 of 82";
      var DisplayItem77 = new IATDisplayItem(77, "sexualattitudes0077.jpeg", 370, 5, 150, 110);
      DisplayItem77.Load();
      ///    while (DisplayItem77.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 79 of 82";
      var DisplayItem78 = new IATDisplayItem(78, "sexualattitudes0078.png", 27, 425, 470, 100);
      DisplayItem78.Load();
      ///    while (DisplayItem78.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 80 of 82";
      var DisplayItem79 = new IATDisplayItem(79, "sexualattitudes0079.png", 237, 371, 50, 50);
      DisplayItem79.Load();
      ///    while (DisplayItem79.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 81 of 82";
      var DisplayItem80 = new IATDisplayItem(80, "sexualattitudes0080.png", 10, 10, 160, 120);
      DisplayItem80.Load();
      ///    while (DisplayItem80.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
      LoadingImagesProgressText.nodeValue = "Loading image 82 of 82";
      var DisplayItem81 = new IATDisplayItem(81, "sexualattitudes0081.png", 375, 10, 160, 120);
      DisplayItem81.Load();
      ///    while (DisplayItem81.imgTag.getAttribute("complete") != "complete")
      ///      setTimeout('b = true', 100);
    
    Display.divTag.removeChild(LoadingImagesProgressElement);
    Display.divTag.removeChild(LoadingImagesElement);
  
    ErrorMark = DisplayItem79;
    ErrorMark.imgTag.id = DisplayItem79.imgTag.id;
    
    var ItemBlocks = new Array();
    var InstructionBlocks = new Array();
    
          InstructionBlocks.push(new IATInstructionBlock(-1));
        
          InstructionBlocks[InstructionBlocks.length - 1].AddScreen(new IATTextInstructionScreen(2, 32, DisplayItem1, DisplayItem0));
        
          InstructionBlocks[InstructionBlocks.length - 1].AddScreen(new IATMockItemInstructionScreen(3, 32, DisplayItem6, DisplayItem2, DisplayItem3, DisplayItem4, DisplayItem5, true, false, false));
        
          InstructionBlocks[InstructionBlocks.length - 1].AddScreen(new IATKeyedInstructionScreen(4, 32, DisplayItem10, DisplayItem7, DisplayItem8, DisplayItem9));
        
          ItemBlocks.push(new IATBlock(ItemBlocks.length + 1, 16, -1));
          ItemBlocks[ItemBlocks.length - 1].BeginBlockEvent = new IATBeginBlock(5, false, DisplayItem11, DisplayItem12, DisplayItem13);
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(6, DisplayItem14, 1, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(7, DisplayItem15, 2, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(8, DisplayItem4, 3, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(9, DisplayItem16, 4, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(10, DisplayItem17, 5, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(11, DisplayItem18, 6, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(12, DisplayItem19, 7, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(13, DisplayItem20, 8, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(14, DisplayItem21, 9, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(15, DisplayItem22, 10, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(16, DisplayItem23, 11, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(17, DisplayItem24, 12, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(18, DisplayItem25, 13, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(19, DisplayItem26, 14, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(20, DisplayItem27, 15, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(21, DisplayItem28, 16, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].EndBlockEvent = new IATEndBlock(22);
        
          InstructionBlocks.push(new IATInstructionBlock(-1));
        
          InstructionBlocks[InstructionBlocks.length - 1].AddScreen(new IATKeyedInstructionScreen(24, 32, DisplayItem32, DisplayItem29, DisplayItem30, DisplayItem31));
        
          ItemBlocks.push(new IATBlock(ItemBlocks.length + 1, 16, -1));
          ItemBlocks[ItemBlocks.length - 1].BeginBlockEvent = new IATBeginBlock(25, false, DisplayItem33, DisplayItem34, DisplayItem35);
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(26, DisplayItem36, 17, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(27, DisplayItem37, 18, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(28, DisplayItem38, 19, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(29, DisplayItem39, 20, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(30, DisplayItem40, 21, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(31, DisplayItem41, 22, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(32, DisplayItem42, 23, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(33, DisplayItem43, 24, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(34, DisplayItem44, 25, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(35, DisplayItem45, 26, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(36, DisplayItem46, 27, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(37, DisplayItem47, 28, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(38, DisplayItem48, 29, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(39, DisplayItem49, 30, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(40, DisplayItem50, 31, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(41, DisplayItem51, 32, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].EndBlockEvent = new IATEndBlock(42);
        
          InstructionBlocks.push(new IATInstructionBlock(2));
        
          InstructionBlocks[InstructionBlocks.length - 1].AddScreen(new IATKeyedInstructionScreen(44, 32, DisplayItem55, DisplayItem52, DisplayItem53, DisplayItem54));
        
          ItemBlocks.push(new IATBlock(ItemBlocks.length + 1, 32, 6));
          ItemBlocks[ItemBlocks.length - 1].BeginBlockEvent = new IATBeginBlock(45, false, DisplayItem56, DisplayItem57, DisplayItem58);
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(46, DisplayItem21, 33, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(47, DisplayItem16, 34, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(48, DisplayItem40, 35, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(49, DisplayItem36, 36, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(50, DisplayItem48, 37, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(51, DisplayItem37, 38, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(52, DisplayItem26, 39, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(53, DisplayItem46, 40, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(54, DisplayItem41, 41, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(55, DisplayItem22, 42, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(56, DisplayItem23, 43, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(57, DisplayItem44, 44, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(58, DisplayItem50, 45, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(59, DisplayItem45, 46, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(60, DisplayItem24, 47, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(61, DisplayItem43, 48, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(62, DisplayItem18, 49, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(63, DisplayItem19, 50, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(64, DisplayItem38, 51, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(65, DisplayItem47, 52, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(66, DisplayItem51, 53, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(67, DisplayItem17, 54, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(68, DisplayItem25, 55, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(69, DisplayItem4, 56, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(70, DisplayItem49, 57, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(71, DisplayItem15, 58, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(72, DisplayItem27, 59, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(73, DisplayItem28, 60, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(74, DisplayItem14, 61, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(75, DisplayItem20, 62, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(76, DisplayItem39, 63, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(77, DisplayItem42, 64, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].EndBlockEvent = new IATEndBlock(78);
        
          ItemBlocks.push(new IATBlock(ItemBlocks.length + 1, 32, 7));
          ItemBlocks[ItemBlocks.length - 1].BeginBlockEvent = new IATBeginBlock(79, false, DisplayItem59, DisplayItem60, DisplayItem61);
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(80, DisplayItem21, 65, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(81, DisplayItem41, 66, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(82, DisplayItem42, 67, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(83, DisplayItem4, 68, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(84, DisplayItem44, 69, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(85, DisplayItem19, 70, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(86, DisplayItem22, 71, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(87, DisplayItem26, 72, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(88, DisplayItem20, 73, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(89, DisplayItem27, 74, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(90, DisplayItem18, 75, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(91, DisplayItem28, 76, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(92, DisplayItem37, 77, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(93, DisplayItem16, 78, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(94, DisplayItem48, 79, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(95, DisplayItem43, 80, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(96, DisplayItem38, 81, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(97, DisplayItem47, 82, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(98, DisplayItem14, 83, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(99, DisplayItem40, 84, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(100, DisplayItem36, 85, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(101, DisplayItem50, 86, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(102, DisplayItem17, 87, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(103, DisplayItem24, 88, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(104, DisplayItem45, 89, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(105, DisplayItem51, 90, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(106, DisplayItem39, 91, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(107, DisplayItem46, 92, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(108, DisplayItem23, 93, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(109, DisplayItem25, 94, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(110, DisplayItem15, 95, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(111, DisplayItem49, 96, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].EndBlockEvent = new IATEndBlock(112);
        
          InstructionBlocks.push(new IATInstructionBlock(-1));
        
          InstructionBlocks[InstructionBlocks.length - 1].AddScreen(new IATKeyedInstructionScreen(114, 32, DisplayItem65, DisplayItem62, DisplayItem63, DisplayItem64));
        
          ItemBlocks.push(new IATBlock(ItemBlocks.length + 1, 16, -1));
          ItemBlocks[ItemBlocks.length - 1].BeginBlockEvent = new IATBeginBlock(115, false, DisplayItem66, DisplayItem67, DisplayItem68);
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(116, DisplayItem50, 97, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(117, DisplayItem43, 98, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(118, DisplayItem45, 99, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(119, DisplayItem39, 100, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(120, DisplayItem41, 101, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(121, DisplayItem49, 102, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(122, DisplayItem48, 103, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(123, DisplayItem47, 104, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(124, DisplayItem46, 105, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(125, DisplayItem40, 106, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(126, DisplayItem37, 107, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(127, DisplayItem42, 108, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(128, DisplayItem51, 109, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(129, DisplayItem36, 110, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(130, DisplayItem38, 111, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(131, DisplayItem44, 112, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].EndBlockEvent = new IATEndBlock(132);
        
          InstructionBlocks.push(new IATInstructionBlock(3));
        
          InstructionBlocks[InstructionBlocks.length - 1].AddScreen(new IATKeyedInstructionScreen(134, 32, DisplayItem72, DisplayItem69, DisplayItem70, DisplayItem71));
        
          ItemBlocks.push(new IATBlock(ItemBlocks.length + 1, 32, 3));
          ItemBlocks[ItemBlocks.length - 1].BeginBlockEvent = new IATBeginBlock(135, false, DisplayItem73, DisplayItem74, DisplayItem75);
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(136, DisplayItem48, 113, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(137, DisplayItem4, 114, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(138, DisplayItem41, 115, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(139, DisplayItem46, 116, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(140, DisplayItem21, 117, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(141, DisplayItem28, 118, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(142, DisplayItem40, 119, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(143, DisplayItem24, 120, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(144, DisplayItem26, 121, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(145, DisplayItem37, 122, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(146, DisplayItem16, 123, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(147, DisplayItem14, 124, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(148, DisplayItem25, 125, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(149, DisplayItem36, 126, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(150, DisplayItem27, 127, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(151, DisplayItem22, 128, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(152, DisplayItem15, 129, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(153, DisplayItem45, 130, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(154, DisplayItem42, 131, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(155, DisplayItem17, 132, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(156, DisplayItem20, 133, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(157, DisplayItem51, 134, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(158, DisplayItem50, 135, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(159, DisplayItem43, 136, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(160, DisplayItem18, 137, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(161, DisplayItem49, 138, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(162, DisplayItem38, 139, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(163, DisplayItem39, 140, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(164, DisplayItem23, 141, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(165, DisplayItem19, 142, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(166, DisplayItem47, 143, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(167, DisplayItem44, 144, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].EndBlockEvent = new IATEndBlock(168);
        
          ItemBlocks.push(new IATBlock(ItemBlocks.length + 1, 32, 4));
          ItemBlocks[ItemBlocks.length - 1].BeginBlockEvent = new IATBeginBlock(169, false, DisplayItem76, DisplayItem77, DisplayItem78);
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(170, DisplayItem15, 145, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(171, DisplayItem20, 146, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(172, DisplayItem23, 147, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(173, DisplayItem22, 148, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(174, DisplayItem26, 149, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(175, DisplayItem4, 150, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(176, DisplayItem45, 151, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(177, DisplayItem28, 152, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(178, DisplayItem19, 153, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(179, DisplayItem16, 154, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(180, DisplayItem50, 155, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(181, DisplayItem48, 156, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(182, DisplayItem40, 157, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(183, DisplayItem49, 158, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(184, DisplayItem25, 159, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(185, DisplayItem44, 160, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(186, DisplayItem47, 161, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(187, DisplayItem43, 162, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(188, DisplayItem37, 163, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(189, DisplayItem14, 164, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(190, DisplayItem41, 165, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(191, DisplayItem46, 166, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(192, DisplayItem18, 167, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(193, DisplayItem27, 168, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(194, DisplayItem21, 169, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(195, DisplayItem51, 170, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(196, DisplayItem39, 171, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(197, DisplayItem36, 172, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(198, DisplayItem24, 173, "Right"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(199, DisplayItem38, 174, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(200, DisplayItem17, 175, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(201, DisplayItem42, 176, "Left"));
        
          ItemBlocks[ItemBlocks.length - 1].EndBlockEvent = new IATEndBlock(202);
        

    var itemBlock;
    var instructionBlock;
    var alternate = CookieUtil.getCookie("Alternate");
    var itemBlockCtr = 0;
    var instructionBlockCtr = 0;
    
          if (alternate == "yes") {
          instructionBlock = InstructionBlocks[0];
          instructionBlock = InstructionBlocks.splice(0, 1);
          } else
          instructionBlock = InstructionBlocks.shift();
          while (instructionBlock.screens.length > 0)
          EventList.push(instructionBlock.screens.shift());
          instructionBlockCtr++;
        
          if (alternate == "yes") {
          itemBlock = ItemBlocks[0];
          ItemBlocks.splice(0, 1);
          } else
          itemBlock = ItemBlocks.shift();
          result = itemBlock.GenerateContents("RandomOrder");
          while (result.length > 0)
          EventList.push(result.shift());
          itemBlockCtr++;
        
          if (alternate == "yes") {
          instructionBlock = InstructionBlocks[0];
          instructionBlock = InstructionBlocks.splice(0, 1);
          } else
          instructionBlock = InstructionBlocks.shift();
          while (instructionBlock.screens.length > 0)
          EventList.push(instructionBlock.screens.shift());
          instructionBlockCtr++;
        
          if (alternate == "yes") {
          itemBlock = ItemBlocks[0];
          ItemBlocks.splice(0, 1);
          } else
          itemBlock = ItemBlocks.shift();
          result = itemBlock.GenerateContents("RandomOrder");
          while (result.length > 0)
          EventList.push(result.shift());
          itemBlockCtr++;
        
          if (alternate == "yes") {
          instructionBlock = 
            InstructionBlocks[2];
          
          instructionBlock = InstructionBlocks.splice(2 - (instructionBlockCtr + 1)
          , 1);
          } else
          instructionBlock = InstructionBlocks.shift();
          while (instructionBlock.screens.length > 0)
          EventList.push(instructionBlock.screens.shift());
          instructionBlockCtr++;
        
          if (alternate == "yes") {
          itemBlock = 
            ItemBlocks[6];
          
          ItemBlocks.splice(6 - (itemBlockCtr + 1)
          , 1);
          } else
          itemBlock = ItemBlocks.shift();
          result = itemBlock.GenerateContents("RandomOrder");
          while (result.length > 0)
          EventList.push(result.shift());
          itemBlockCtr++;
        
          if (alternate == "yes") {
          itemBlock = 
            ItemBlocks[7];
          
          ItemBlocks.splice(7 - (itemBlockCtr + 1)
          , 1);
          } else
          itemBlock = ItemBlocks.shift();
          result = itemBlock.GenerateContents("RandomOrder");
          while (result.length > 0)
          EventList.push(result.shift());
          itemBlockCtr++;
        
          if (alternate == "yes") {
          instructionBlock = InstructionBlocks[0];
          instructionBlock = InstructionBlocks.splice(0, 1);
          } else
          instructionBlock = InstructionBlocks.shift();
          while (instructionBlock.screens.length > 0)
          EventList.push(instructionBlock.screens.shift());
          instructionBlockCtr++;
        
          if (alternate == "yes") {
          itemBlock = ItemBlocks[0];
          ItemBlocks.splice(0, 1);
          } else
          itemBlock = ItemBlocks.shift();
          result = itemBlock.GenerateContents("RandomOrder");
          while (result.length > 0)
          EventList.push(result.shift());
          itemBlockCtr++;
        
          if (alternate == "yes") {
          instructionBlock = 
            InstructionBlocks[3];
          
          instructionBlock = InstructionBlocks.splice(3 - (instructionBlockCtr + 1)
          , 1);
          } else
          instructionBlock = InstructionBlocks.shift();
          while (instructionBlock.screens.length > 0)
          EventList.push(instructionBlock.screens.shift());
          instructionBlockCtr++;
        
          if (alternate == "yes") {
          itemBlock = 
            ItemBlocks[3];
          
          ItemBlocks.splice(3 - (itemBlockCtr + 1)
          , 1);
          } else
          itemBlock = ItemBlocks.shift();
          result = itemBlock.GenerateContents("RandomOrder");
          while (result.length > 0)
          EventList.push(result.shift());
          itemBlockCtr++;
        
          if (alternate == "yes") {
          itemBlock = 
            ItemBlocks[4];
          
          ItemBlocks.splice(4 - (itemBlockCtr + 1)
          , 1);
          } else
          itemBlock = ItemBlocks.shift();
          result = itemBlock.GenerateContents("RandomOrder");
          while (result.length > 0)
          EventList.push(result.shift());
          itemBlockCtr++;
        
    EventList.push(new IATSubmitEvent(EventList.length));
  
    var ClickToStartElement = document.createElement("h4");
    var ClickToStartText = document.createTextNode("Click Here to Begin");
    ClickToStartElement.appendChild(ClickToStartText);
    Display.divTag.appendChild(ClickToStartElement);
    currentHandler = function() {
    Display.divTag.removeChild(ClickToStartElement);
    EventUtil.removeHandler(Display.divTag, "click", currentHandler);
    EventList[EventCtr].Execute();
    }
    EventUtil.addHandler(Display.divTag, "click", currentHandler);
  