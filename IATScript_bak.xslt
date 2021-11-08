﻿<xsl:stylesheet
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
    exclude-result-prefixes="xs">
  <xsl:output method="text" encoding="UTF-8" />
  <xsl:variable name="root" select="/" />

  <xsl:template match="ConfigFile" >
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
    var ImageLoadCtr = 0;
    var NumImages;
    var ImageLoadStatusTextElement;
    var ClickToStartElement;
    var ClickToStartText;

    var EventUtil = {
    addHandler: function(element, type, handler) {
    if (element.addEventListener) {
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
    this.src = "<xsl:value-of select="concat(./ServerURL, ./ClientID, '/', ./IATName, '/')"/>" + src;
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
    EventUtil.addHandler(this.img, "load", function() {
    ImageLoadCtr++;
    if (ImageLoadCtr == NumImages)
    OnImageLoadComplete();
    else
    ImageLoadStatusTextElement.nodeValue = "Loading image #" + (ImageLoadCtr + 1).toString() + " of " + NumImages.toString();
    });
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
    for (var ctr = 0; ctr &lt; this.displayItems.length; ctr++)
    {
    if (this.displayItems[ctr].id == di.id)
    {
    this.displayItems[ctr].Hide();
    this.displayItems.splice(ctr, 1);
    }
    }
    },

    Clear : function() {
    for (var ctr = 0; ctr &lt; this.displayItems.length; ctr++)
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
    var numItemsInput = document.createElement("input");
    numItemsInput.name = "NumItems";
    numItemsInput.type = "hidden";
    numItemsInput.value = currentItemNum.toString();
    Display.divTag.appendChild(numItemsInput);
    var form = document.getElementById("IATForm");
    form.submit();
    }};


    function IATItem(id, stimulus, itemNum, keyedDir)
    {
    if (keyedDir == "Left")
    {
    IATEvent.call(this, id, function(event)
    {
    event = EventUtil.getEvent(event);
    var keyCode = EventUtil.getCharCode(event);
    if ((keyCode == Display.leftResponseKeyCodeUpper) || (keyCode == Display.leftResponseKeyCodeLower))
    {
    if (!inPracticeBlock)
    {
    var latencyItemName = document.createElement("input");
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
    event = EventUtil.getEvent(event);
    var keyCode = EventUtil.getCharCode(event);
    if ((keyCode == Display.rightResponseKeyCodeUpper) || (keyCode == Display.rightResponseKeyCodeLower))
    {
    if (!inPracticeBlock)
    {
    var latencyItemName = document.createElement("input");
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
    while ((new Date()).getTime() - 100 &lt; Display.startTime);
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
    event = EventUtil.getEvent(event);
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
    var currItemNdx, lastItemNdx = -1;
    if (randomization == "None")
    {
    for (ctr = 0; ctr &lt; Items.length; ctr++)
    result.push(this.Items[ctr]);
    }
    else if (randomization == "RandomOrder")
    {
    var tempItems = new Array();
    for (ctr = 0; ctr &lt; this.Items.length; ctr++)
    tempItems.push(this.Items[ctr]);
    for (ctr = 0; ctr &lt; this.Items.length; ctr++)
    {
    var ndx = Math.floor(Math.random() * tempItems.length);
    result.push(tempItems[ndx]);
    tempItems.splice(ndx, 1);
    }
    } else if (randomization == "SetNumberOfPresentations")
    {
    for (ctr = 0; ctr &lt; this.numPresentations; ctr++)
    {
    currItemNdx = Math.floor(Math.random() * this.Items.length);
    while (currItemNdx == lastItemNdx)
    currItemNdx = Math.floor(Math.random() * this.Items.length);
    result.push(this.Items[currItemNdx]);
    lastItemNdx = currItemNdx;
    }
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

    function OnImageLoadComplete()
    {
    while (Display.divTag.firstChild)
    Display.divTag.removeChild(Display.divTag.firstChild);
    ClickToStartElement = document.createElement("h4");
    ClickToStartText = document.createTextNode("Click Here to Begin");
    ClickToStartElement.appendChild(ClickToStartText);
    Display.divTag.appendChild(ClickToStartElement);
    currentHandler = function() {
    Display.divTag.removeChild(ClickToStartElement);
    EventUtil.removeHandler(Display.divTag, "click", currentHandler);
    EventList[EventCtr].Execute();
    }
    EventUtil.addHandler(Display.divTag, "click", currentHandler);
    Display.divTag.tabIndex = -1;
    Display.divTag.focus();
    var bodyTag = document.getElementById("bodyID");
    EventUtil.addHandler(bodyTag, "click", function() {
    Display.divTag.tabIndx = -1;
    Display.divTag.focus();
    });
    var containerDiv = document.getElementById("IATContainerDiv");
    EventUtil.addHandler(containerDiv, "click", function() {
    Display.divTag.tabIndex = -1;
    Display.divTag.focus();
    });
    }

    var Display = new IATDisplay(<xsl:value-of select="concat(./IATLayout/InteriorWidth, ', ', ./IATLayout/InteriorHeight, ', ', ./LeftResponseASCIIKeyCodeUpper, ', ', ./LeftResponseASCIIKeyCodeLower, ', ', ./RightResponseASCIIKeyCodeUpper, ', ', ./RightResponseASCIIKeyCodeLower)" />);
    <xsl:call-template name="GenerateImageLoad" >
      <xsl:with-param name="ImageListNode" select="./DisplayItemList" />
    </xsl:call-template>
    ErrorMark = <xsl:value-of select="concat('DisplayItem', ./ErrorMarkID)" />;
    ErrorMark.imgTag.id = <xsl:value-of select="concat('DisplayItem', ./ErrorMarkID)" />.imgTag.id;
    <xsl:call-template name="GenerateEventInit" >
      <xsl:with-param name="EventListNode" select="./IATEventList" />
      <xsl:with-param name="randomization" select="./RandomizationType" />
    </xsl:call-template>

  </xsl:template>

  <xsl:template name="GenerateImageLoad" >
    <xsl:param name="ImageListNode" />
    <xsl:variable name="numImages" select="count($ImageListNode/*)" />
    NumImages = <xsl:value-of select="$numImages" />;
    ImageLoadCtr = 0;
    var LoadingImagesElement = document.createElement("h3");
    var LoadingImagesText = document.createTextNode("Please Wait");
    LoadingImagesElement.appendChild(LoadingImagesText);
    var LoadingImagesProgressElement = document.createElement("h4");
    ImageLoadStatusTextElement = document.createTextNode("");
    ImageLoadStatusTextElement.nodeValue = "Loading image #1 of " + NumImages.toString();
    LoadingImagesProgressElement.appendChild(ImageLoadStatusTextElement);
    Display.divTag.appendChild(LoadingImagesElement);
    Display.divTag.appendChild(LoadingImagesProgressElement);
    <xsl:for-each select="$ImageListNode/*" >
      var <xsl:value-of select="concat('DisplayItem', ./ID)" /> = new IATDisplayItem(<xsl:value-of select="concat(./ID, ', &quot;', ./Filename, '&quot;, ', ./X, ', ', ./Y, ', ', ./Width, ', ', ./Height)" />);
    </xsl:for-each>
    <xsl:for-each select="$ImageListNode/*" >
      <xsl:value-of select="concat('DisplayItem', ./ID, '.Load();&#x0A;')"/>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="GenerateEventInit" >
    <xsl:param name="EventListNode" />
    <xsl:param name="randomization" />
    var ItemBlocks = new Array();
    var InstructionBlocks = new Array();
    <xsl:for-each select="$EventListNode/*" >
      <xsl:choose>
        <xsl:when test="@EventType eq 'IATItem'" >
          var KeyedDir;
          <xsl:if test="./SpecifierID ne '-1'" >
            var SpecifierResult = document.getElementById("<xsl:value-of select="concat('DynamicKey', ./SpecifierID)" />").value;
            if (SpecifierResult == "True")
            KeyedDir = "<xsl:value-of select="./KeyedDir" />";
            else
            {
            KeyedDir = "<xsl:value-of select="./KeyedDir" />";
            if (KeyedDir == "Left")
            KeyedDir = "Right";
            else
            KeyedDir = "Left";
            }
          </xsl:if>
          <xsl:if test="./SpecifierID eq '-1'" >
            KeyedDir = "<xsl:value-of select="./KeyedDir" />";
          </xsl:if>
          ItemBlocks[ItemBlocks.length - 1].Items.push(new IATItem(<xsl:value-of select="concat(position(), ', ', 'DisplayItem', ./StimulusDisplayID, ', ', ./ItemNum, ',  KeyedDir')" />));
        </xsl:when>
        <xsl:when test="@EventType eq 'BeginIATBlock'" >
          ItemBlocks.push(new IATBlock(ItemBlocks.length + 1, <xsl:value-of select="concat(./NumPresentations, ', ', ./AlternatedWith)" />));
          ItemBlocks[ItemBlocks.length - 1].BeginBlockEvent = new IATBeginBlock(<xsl:value-of select="concat(position(), ', ', lower-case(./PracticeBlock), ', DisplayItem', ./LeftResponseDisplayID, ', DisplayItem', ./RightResponseDisplayID, ', DisplayItem', ./InstructionsDisplayID)" />);

          <xsl:variable name="blockLength" select="./NumItems" as="xs:integer" />
          <xsl:variable name="itemsInBlock" select="following::IATEvent[position() le $blockLength]" />
          <xsl:call-template name="GenerateBlockLoad" >
            <xsl:with-param name="items" select="$itemsInBlock" />
            <xsl:with-param name="startPosition" select="xs:integer(position())" as="xs:integer" />
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="@EventType eq 'EndIATBlock'" >
          ItemBlocks[ItemBlocks.length - 1].EndBlockEvent = new IATEndBlock(<xsl:value-of select="position()" />);
        </xsl:when>
        <xsl:when test="@EventType eq 'BeginInstructionBlock'" >
          InstructionBlocks.push(new IATInstructionBlock(<xsl:value-of select="./AlternatedWith" />));
        </xsl:when>
        <xsl:when test="@EventType eq 'TextInstructionScreen'" >
          InstructionBlocks[InstructionBlocks.length - 1].AddScreen(new IATTextInstructionScreen(<xsl:value-of select="concat(position(), ', ', ./ContinueASCIIKeyCode, ', DisplayItem', ./ContinueInstructionsDisplayID, ', DisplayItem', ./InstructionsDisplayID)" />));
        </xsl:when>
        <xsl:when test="@EventType eq 'KeyedInstructionScreen'" >
          InstructionBlocks[InstructionBlocks.length - 1].AddScreen(new IATKeyedInstructionScreen(<xsl:value-of select="concat(position(), ', ', ./ContinueASCIIKeyCode, ', DisplayItem', ./ContinueInstructionsDisplayID, ', DisplayItem', ./InstructionsDisplayID, ', DisplayItem', ./LeftResponseDisplayID, ', DisplayItem', ./RightResponseDisplayID)" />));
        </xsl:when>
        <xsl:when test="@EventType eq 'MockItemInstructionScreen'" >
          InstructionBlocks[InstructionBlocks.length - 1].AddScreen(new IATMockItemInstructionScreen(<xsl:value-of select="concat(position(), ', ', ./ContinueASCIIKeyCode, ', DisplayItem', ./ContinueInstructionsDisplayID, ', DisplayItem', ./LeftResponseDisplayID, ', DisplayItem', ./RightResponseDisplayID, ', DisplayItem', ./StimulusDisplayID, ', DisplayItem', ./InstructionsDisplayID, ', ', lower-case(./ErrorMarkIsDisplayed), ', ', lower-case(./OutlineLeftResponse), ', ', lower-case(./OutlineRightResponse))" />));
        </xsl:when>
      </xsl:choose>
    </xsl:for-each>

    var itemBlock;
    var instructionBlock;
    var alternate = document.getElementById("Alternate").value;
    var itemBlockCtr = 0;
    var instructionBlockCtr = 0;
    var numAlternatedItemBlocks = 0;
    var numAlternatedInstructionBlocks = 0;
    <xsl:variable name="blockStarters" select="('BeginIATBlock', 'BeginInstructionBlock')" />
    <xsl:for-each select="$EventListNode/IATEvent[some $type in $blockStarters satisfies $type eq @EventType]" >
      <xsl:choose>
        <xsl:when test="@EventType eq 'BeginIATBlock'" >
          if (alternate == "yes") {
          <xsl:if test="./AlternatedWith ne '-1'" >
            <xsl:choose>
              <xsl:when test="./AlternatedWith gt ./BlockNum" >
                itemBlock = ItemBlocks[<xsl:value-of select="./AlternatedWith" /> - (itemBlockCtr + 1)];
                ItemBlocks.splice(<xsl:value-of select="./AlternatedWith" /> - (itemBlockCtr + 1), 1);
                numAlternatedItemBlocks++;
              </xsl:when>
              <xsl:when test="./AlternatedWith lt ./BlockNum" >
                itemBlock = ItemBlocks[<xsl:value-of select="./AlternatedWith" /> - (numAlternatedItemBlocks + 1)];
                ItemBlocks.splice(<xsl:value-of select="./AlternatedWith" /> - (numAlternatedItemBlocks + 1), 1);
                numAlternatedItemBlocks++;
              </xsl:when>
            </xsl:choose>
          </xsl:if>
          <xsl:if test="./AlternatedWith eq '-1'" >
            itemBlock = ItemBlocks[numAlternatedItemBlocks];
            ItemBlocks.splice(numAlternatedItemBlocks, 1);
          </xsl:if>
          }
          else
          itemBlock = ItemBlocks.shift();
          result = itemBlock.GenerateContents("<xsl:value-of select="$randomization" />");
          while (result.length &gt; 0)
          EventList.push(result.shift());
          itemBlockCtr++;
        </xsl:when>
        <xsl:when test="@EventType eq 'BeginInstructionBlock'" >
          <xsl:variable name="precedingNodes" select="preceding-sibling::node()" />
          <xsl:variable name="blockNum" select="count($precedingNodes[@EventType eq 'BeginInstructionBlock'])" as="xs:integer" />
          if (alternate == "yes") {
          <xsl:if test="./AlternatedWith ne '-1'" >
            <xsl:choose>
              <xsl:when test="xs:integer(./AlternatedWith) gt $blockNum" >
                instructionBlock = InstructionBlocks[<xsl:value-of select="./AlternatedWith" /> - (instructionBlockCtr + 1)];
                InstructionBlocks.splice(<xsl:value-of select="./AlternatedWith" /> - (instructionBlockCtr + 1), 1);
                numAlternatedInstructionBlocks++;
              </xsl:when>
              <xsl:when test="xs:integer(./AlternatedWith) lt $blockNum" >
                instructionBlock = InstructionBlocks[<xsl:value-of select="./AlternatedWith" /> - (numAlternatedInstructionBlocks + 2)];
                InstructionBlocks.splice(<xsl:value-of select="./AlternatedWith" /> - (numAlternatedInstructionBlocks + 2), 1);
                numAlternatedInstructionBlocks++;
              </xsl:when>
            </xsl:choose>
          </xsl:if>
          <xsl:if test="./AlternatedWith eq '-1'" >
            instructionBlock = InstructionBlocks[numAlternatedInstructionBlocks];
            InstructionBlocks.splice(numAlternatedInstructionBlocks, 1);
          </xsl:if>
          } else
          instructionBlock = InstructionBlocks.shift();
          while (instructionBlock.screens.length &gt; 0)
          EventList.push(instructionBlock.screens.shift());
          instructionBlockCtr++;
        </xsl:when>
      </xsl:choose>
    </xsl:for-each>
    EventList.push(new IATSubmitEvent(EventList.length));
  </xsl:template>

  <xsl:template name="GenerateBlockLoad" >
    <xsl:param name="items" />

    <xsl:param name="startPosition" as="xs:integer" />

    var KeyedDir;
    var KeyedDirInput;
    var DefaultKey;
    var Items = new Array();
    var FreeItemIDs = new Array();
    <xsl:for-each select="$items" >
      <xsl:variable name="currentItem" select="." />
      <xsl:if test="xs:integer(./SpecifierID) eq -1" >
        KeyedDir = "<xsl:value-of select="./KeyedDir" />";
        Items[Items.length - 1].push(new IATItem(<xsl:value-of select="concat(xs:string(position() + $startPosition), ', ', 'DisplayItem', ./StimulusDisplayID, ', ', ./ItemNum, ',  KeyedDir')" />));
      </xsl:if>
      <xsl:if test="xs:integer(./SpecifierID) ne -1" >

        <xsl:if test="./KeyedDir eq 'DynamicRight'" >
          DefaultKey = "Right";
        </xsl:if>
        <xsl:if test="./KeyedDir eq 'DynamicLeft'" >
          DefaultKey = "Left";
        </xsl:if>

        <xsl:variable name="specifier" select="//DynamicSpecifier[./ID eq $currentItem/SpecifierID]" />
        <xsl:choose>
          <xsl:when test="$specifier/@SpecifierType eq 'TrueFalse'" >
            KeyedDirInput = document.getElementById("<xsl:value-of select="concat('DynamicKey', 
$specifier/ID)" />");
            if (KeyedDirInput.value == "True")
            KeyedDir = DefaultKey;
            else if (DefaultKey == "Right")
            KeyedDir = "Left";
            else
            KeyedDir = "Right;
            Items[Items.length - 1].push(new IATItem(<xsl:value-of select="concat(xs:string(position() + $startPosition), ', ', 'DisplayItem', ./StimulusDisplayID, ', ', ./ItemNum, ',  KeyedDir')" />));
          </xsl:when>
          <xsl:when test="$specifier/@SpecifierType eq 'Mask'" >

            KeyedDirInput = document.getElementById("<xsl:value-of select="concat('DynamicKey', $specifier/ID)" />);
            if (KeyedDirInput.value == "True")
            KeyedDir = DefaultKey;
            else if (DefaultKey == "Right")
            KeyedDir = "Left";
            else
            KeyedDir = "Right";
            ItemBlocks[ItemBlocks.length - 1].push(new IATItem(<xsl:value-of select="concat(xs:string(position() + $startPosition), ', ', 'DisplayItem', ./StimulusDisplayID, ', ', ./ItemNum, ',  KeyedDir')" />));
          </xsl:when>
          <xsl:when test="$specifier/@pecifierType eq 'Selection'" >
            FreeItemIDs.push(<xsl:value-of select="xs:string(position() + $startPosition)" />);
          </xsl:when>
        </xsl:choose>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//DynamicSpecifier[(some $i in $items satisfies $i/SpecifierID = ID) and (@SpecifierType eq 'Selection')]" >
      <xsl:variable name="specifier" select="." />
      var SelectedItem = document.getElementById("<xsl:value-of select="concat('DynamicKey', ./ID)" />").value;
      var RandomItem = SelectedItem;

      var SelectionStimulusArray = new Array();

      var SelectionKeyedDir;
      var SelectionStimulus;
      var SelectionItemNum;
      <xsl:for-each select="//IATEvent[(SpecifierID eq $specifier/ID) and (@EventType eq 'IATItem')]" >
        SelectionStimulusArray.push(new Array(<xsl:value-of select="concat(./SpecifierArg, ', DisplayItem', ./StimulusDisplayID, ', ', ./KeyedDir, ', ', ./ItemNum)" />
        ));
      </xsl:for-each>

      for (var ctr = 0; ctr &lt; SelectedStimulusArray.length; ctr++)				if (parseInt(SelectedItem, 10) == SelectedStimulusArray[ctr][0])				{
      SelectionKeyedDir = SelectedStimulusArray[ctr][2];
      SelectionStimulus = SelectedStimulusArray[ctr][1];
      SelectionItemNum = SelectedStimulusArray[ctr][3];
      if (SelectionKeyedDir == "DynamicLeft";
      SelectionKeyedDir = "Left";
      else
      SelectionKeyedDir = "Right";
      }
      Items.push(new IATItem(FreeItemIDs.shift(), SelectionStimulus, SelectionItemNum, SelectionKeyedDir));
      while (RandomItem == SelectedItem) {
      var randomNum = Math.floor(Math.random() * <xsl:value-of select="count(./KeySpecifier)" />);
      <xsl:for-each select="./KeySpecifier" >
        <xsl:variable name="key" select="." />
        if (randomNum == <xsl:value-of select="./position()" />)
        RandomItem = <xsl:value-of select="." />);
      </xsl:for-each>
      for (var ctr = 0; ctr &lt; SelectedStimulusArray.length; ctr++)
      if (parseInt(SelectedItem, 10) == SelectedStimulusArray[ctr][0])
      {
      SelectionKeyedDir = SelectedStimulusArray[ctr][2];
      SelectionStimulus = SelectedStimulusArray[ctr][1];
      SelectionItemNum = SelectedStimulusArray[ctr][3];
      if (SelectionKeyedDir == "DynamicLeft";
      SelectionKeyedDir = "Left";
      else
      SelectionKeyedDir = "Right";
      }
      Items.push(new IATItem(FreeItemIDs.shift(), SelectionStimulus, SelectionItemNum, SelectionKeyedDir));
    </xsl:for-each>		for (var ctr = 0; ctr &lt; Items.length; ctr++)
    {
    var randNum = Math.floor(Math.random() * Items.length);
    ItemBlocks[ItemBlocks.length - 1].Items.push(Items.shift());
    }
  </xsl:template>
</xsl:stylesheet>