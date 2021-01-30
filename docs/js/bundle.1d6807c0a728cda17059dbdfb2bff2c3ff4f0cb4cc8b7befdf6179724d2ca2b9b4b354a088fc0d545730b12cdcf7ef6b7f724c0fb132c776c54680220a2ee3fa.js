// global variables;
const doc = document.documentElement;
const toggleId = 'toggle';
const showId = 'show';
const menu = 'menu';

// defined in config.toml
const parentURL = 'https://passivepicasso.github.io/ThunderKit/docs/';

// defined in i18n / translation files
const quickLinks = 'Quick links';
const searchResultsLabel = 'Search Results';
const shortSearchQuery = 'Query is too short'
const typeToSearch = 'Type to search';
const noMatchesFound = 'No matches found';

;
function isObj(obj) {
  return (obj && typeof obj === 'object' && obj !== null) ? true : false;
}

function createEl(element = 'div') {
  return document.createElement(element);
}

function emptyEl(el) {
  while(el.firstChild)
  el.removeChild(el.firstChild);
}

function elem(selector, parent = document){
  let elem = isObj(parent) ? parent.querySelector(selector) : false;
  return elem ? elem : false;
}

function elems(selector, parent = document) {
  let elems = isObj(parent) ? parent.querySelectorAll(selector) : [];
  return elems.length ? elems : false;
}

function pushClass(el, targetClass) {
  if (isObj(el) && targetClass) {
    let elClass = el.classList;
    elClass.contains(targetClass) ? false : elClass.add(targetClass);
  }
}

function deleteClass(el, targetClass) {
  if (isObj(el) && targetClass) {
    let elClass = el.classList;
    elClass.contains(targetClass) ? elClass.remove(targetClass) : false;
  }
}

function modifyClass(el, targetClass) {
  if (isObj(el) && targetClass) {
    const elClass = el.classList;
    elClass.contains(targetClass) ? elClass.remove(targetClass) : elClass.add(targetClass);
  }
}

function containsClass(el, targetClass) {
  if (isObj(el) && targetClass && el !== document ) {
    return el.classList.contains(targetClass) ? true : false;
  }
}

function isChild(node, parentClass) {
  let objectsAreValid = isObj(node) && parentClass && typeof parentClass == 'string';
  return (objectsAreValid && node.closest(parentClass)) ? true : false;
}

function elemAttribute(elem, attr, value = null) {
  if (value) {
    elem.setAttribute(attr, value);
  } else {
    value = elem.getAttribute(attr);
    return value ? value : false;
  }
}

function deleteChars(str, subs) {
  let newStr = str;
  if (Array.isArray(subs)) {
    for (let i = 0; i < subs.length; i++) {
      newStr = newStr.replace(subs[i], '');
    }
  } else {
    newStr = newStr.replace(subs, '');
  }
  return newStr;
}

function isBlank(str) {
  return (!str || str.trim().length === 0);
}

function isMatch(element, selectors) {
  if(isObj(element)) {
    if(selectors.isArray) {
      let matching = selectors.map(function(selector){
        return element.matches(selector)
      })
      return matching.includes(true);
    }
    return element.matches(selectors)
  }
}

function closestInt(goal, collection) {
  const closest = collection.reduce(function(prev, curr) {
    return (Math.abs(curr - goal) < Math.abs(prev - goal) ? curr : prev);
  });
  return closest;
}

function hasClasses(el) {
  if(isObj(el)) {
    const classes = el.classList;
    return classes.length
  }
}

function wrapEl(el, wrapper) {
  el.parentNode.insertBefore(wrapper, el);
  wrapper.appendChild(el);
}

function wrapText(text, context, wrapper = 'mark') {
  let open = `<${wrapper}>`;
  let close = `</${wrapper}>`;
  function wrap(context) {
    let c = context.innerHTML;
    let pattern = new RegExp(text, "gi");
    let matches = text.length ? c.match(pattern) : null;

    if(matches) {
      matches.forEach(function(matchStr){
        c = c.replaceAll(matchStr, `${open}${matchStr}${close}`);
        context.innerHTML = c;
      });
    }
  }

  const contents = ["h1", "h2", "h3", "h4", "h5", "h6", "p", "code", "td"];

  contents.forEach(function(c){
    const cs = elems(c, context);
    if(cs.length) {
      cs.forEach(function(cx, index){
        if(cx.children.length >= 1) {
          Array.from(cx.children).forEach(function(child){
            wrap(child);
          })
          wrap(cx);
        } else {
          wrap(cx);
        }
        // sanitize urls and ids
      });
    }
  });

  const hyperLinks = elems('a');
  if(hyperLinks) {
    hyperLinks.forEach(function(link){
      const href = link.href.replaceAll(encodeURI(open), "").replaceAll(encodeURI(close), "");
      link.href = href;
    });
  }
}

function parseBoolean(string) {
  let bool;
  string = string.trim().toLowerCase();
  switch (string) {
    case 'true':
      return true;
    case 'false':
      return false;
    default:
      return undefined;
  }
};

function loadSvg(file, parent, path = 'icons/') {
  const link = `${parentURL}${path}${file}.svg`;
  fetch(link)
  .then((response) => {
    return response.text();
  })
  .then((data) => {
    parent.innerHTML = data;
  });
}

function copyToClipboard(str) {
  let copy, selection, selected;
  copy = createEl('textarea');
  copy.value = str;
  copy.setAttribute('readonly', '');
  copy.style.position = 'absolute';
  copy.style.left = '-9999px';
  selection = document.getSelection();
  doc.appendChild(copy);
  // check if there is any selected content
  selected = selection.rangeCount > 0 ? selection.getRangeAt(0) : false;
  copy.select();
  document.execCommand('copy');
  doc.removeChild(copy);
  if (selected) { // if a selection existed before copying
    selection.removeAllRanges(); // unselect existing selection
    selection.addRange(selected); // restore the original selection
  }
}
;
const codeActionButtons = [
  {
    icon: 'copy', 
    id: 'copy',
    title: 'Copy Code',
    show: true
  },
  {
    icon: 'order',
    id: 'lines',
    title: 'Toggle Line Numbers',
    show: true 
  },
  {
    icon: 'carly',
    id: 'wrap',
    title: 'Toggle Line Wrap',
    show: false
  },
  {
    icon: 'expand',
    id: 'expand',
    title: 'Toggle code block expand',
    show: false 
  }
];

const body = elem('body');
const maxLines = parseInt(body.dataset.code);
const copyId = 'panel_copy';
const wrapId = 'panel_wrap';
const linesId = 'panel_lines';
const panelExpand = 'panel_expand';
const panelExpanded = 'panel_expanded';
const panelHide = 'panel_hide';
const panelFrom = 'panel_from';
const panelBox = 'panel_box';
const fullHeight = 'initial';
const highlightWrap = 'highlight_wrap'

function wrapOrphanedPreElements() {
  const pres = elems('pre');
  Array.from(pres).forEach(function(pre){
    const parent = pre.parentNode;
    const isOrpaned = !containsClass(parent, 'highlight');
    if(isOrpaned) {
      const preWrapper = createEl();
      preWrapper.className = 'highlight';
      const outerWrapper = createEl();
      outerWrapper.className = highlightWrap;
      wrapEl(pre, preWrapper);
      wrapEl(preWrapper, outerWrapper);
    }
  })
  /*
  @Todo
  1. Add UI control to orphaned blocks
  */
}

wrapOrphanedPreElements();

function codeBlocks() {
  const markedCodeBlocks = elems('code');
  const blocks = Array.from(markedCodeBlocks).filter(function(block){
    return hasClasses(block) && !Array.from(block.classList).includes('noClass');
  }).map(function(block){
    return block
  });
  return blocks;
}

function codeBlockFits(block) {
  // return false if codeblock overflows
  const blockWidth = block.offsetWidth;
  const highlightBlockWidth = block.parentNode.parentNode.offsetWidth;
  return blockWidth <= highlightBlockWidth ? true : false;
}

function maxHeightIsSet(elem) {
  let maxHeight = elem.style.maxHeight;
  return maxHeight.includes('px')
}

function restrainCodeBlockHeight(lines) {
  const lastLine = lines[maxLines-1];
  let maxCodeBlockHeight = fullHeight;
  if(lastLine) {
    const lastLinePos = lastLine.offsetTop;
    if(lastLinePos !== 0) {
      maxCodeBlockHeight = `${lastLinePos}px`;
      const codeBlock = lines[0].parentNode;
      const outerBlock = codeBlock.closest('.highlight');
      const isExpanded = containsClass(outerBlock, panelExpanded);
      if(!isExpanded) {
        codeBlock.dataset.height = maxCodeBlockHeight;
        codeBlock.style.maxHeight = maxCodeBlockHeight;
      }
    }
  }
}

const blocks = codeBlocks();

function collapseCodeBlock(block) {
  const lines = elems('.ln', block);
  const codeLines = lines.length;
  if (codeLines > maxLines) {
    const expandDot = createEl()
    pushClass(expandDot, panelExpand);
    pushClass(expandDot, panelFrom);
    expandDot.title = "Toggle code block expand";
    expandDot.textContent = "...";
    const outerBlock = block.closest('.highlight');
    window.setTimeout(function(){
      const expandIcon = outerBlock.nextElementSibling.lastElementChild;
      deleteClass(expandIcon, panelHide);
    }, 150)

    restrainCodeBlockHeight(lines);
    const highlightElement = block.parentNode.parentNode;
    highlightElement.appendChild(expandDot);
  }
}

blocks.forEach(function(block){
  collapseCodeBlock(block);
})

function actionPanel() {
  const panel = createEl();
  panel.className = panelBox;

  codeActionButtons.forEach(function(button) {
    // create button
    const btn = createEl('a');
    btn.href = '#';
    btn.title = button.title;
    btn.className = `icon panel_icon panel_${button.id}`;
    button.show ? false : pushClass(btn, panelHide);
    // load icon inside button
    loadSvg(button.icon, btn);
    // append button on panel
    panel.appendChild(btn);
  });

  return panel;
}

function toggleLineNumbers(elems) {
  elems.forEach(function (elem, index) {
    // mark the code element when there are no lines
    modifyClass(elem, 'pre_nolines')
  });
  restrainCodeBlockHeight(elems);
}

function toggleLineWrap(elem) {
  modifyClass(elem, 'pre_wrap');
  // retain max number of code lines on line wrap
  const lines = elems('.ln', elem);
  restrainCodeBlockHeight(lines);
}

function copyCode(codeElement) {
  lineNumbers = elems('.ln', codeElement);
  // remove line numbers before copying
  if(lineNumbers.length) {
    lineNumbers.forEach(function(line){
      line.remove();
    });
  }

  const codeToCopy = codeElement.textContent;
  // copy code
  copyToClipboard(codeToCopy);
}

function disableCodeLineNumbers(block){
  const lines = elems('.ln', block)
  toggleLineNumbers(lines);
}

(function codeActions(){
  const blocks = codeBlocks();

  const highlightWrapId = highlightWrap;
  blocks.forEach(function(block){
    // disable line numbers if disabled globally
    const showLines = elem('body').dataset.lines;
    parseBoolean(showLines) === false ? disableCodeLineNumbers(block) : false;

    const highlightElement = block.parentNode.parentNode;
    // wrap code block in a div
    const highlightWrapper = createEl();
    highlightWrapper.className = highlightWrapId;
    wrapEl(highlightElement, highlightWrapper);

    const panel = actionPanel();
    // show wrap icon only if the code block needs wrapping
    const wrapIcon = elem(`.${wrapId}`, panel);
    codeBlockFits(block) ? false : deleteClass(wrapIcon, panelHide);

    // append buttons 
    highlightWrapper.appendChild(panel);
  });

  function isItem(target, id) {
    // if is item or within item
    return target.matches(`.${id}`) || target.closest(`.${id}`);
  }

  function showActive(target, targetClass,activeClass = 'active') {
    const active = activeClass;
    const targetElement = target.matches(`.${targetClass}`) ? target : target.closest(`.${targetClass}`);

    deleteClass(targetElement, active);
    setTimeout(function() {
      modifyClass(targetElement, active)
    }, 50)
  }

  doc.addEventListener('click', function(event){
    // copy code block
    const target = event.target;
    const isCopyIcon = isItem(target, copyId);
    const isWrapIcon = isItem(target, wrapId);
    const isLinesIcon = isItem(target, linesId);
    const isExpandIcon = isItem(target, panelExpand);
    const isActionable = isCopyIcon || isWrapIcon || isLinesIcon || isExpandIcon;

    if(isActionable) {
      event.preventDefault();
      showActive(target, 'icon');
      const codeElement = target.closest(`.${highlightWrapId}`).firstElementChild.firstElementChild;
      let lineNumbers = elems('.ln', codeElement);

      isWrapIcon ? toggleLineWrap(codeElement) : false;

      isLinesIcon ? toggleLineNumbers(lineNumbers) : false;

      if (isExpandIcon) {
        let thisCodeBlock = codeElement.firstElementChild;
        const outerBlock = thisCodeBlock.closest('.highlight');
        if(maxHeightIsSet(thisCodeBlock)) {
          thisCodeBlock.style.maxHeight = fullHeight;
          // mark code block as expanded
          pushClass(outerBlock, panelExpanded)
        } else {
          thisCodeBlock.style.maxHeight = thisCodeBlock.dataset.height;
          // unmark code block as expanded
          deleteClass(outerBlock, panelExpanded)
        }
      }

      if(isCopyIcon) {
        // clone code element
        const codeElementClone = codeElement.cloneNode(true);
        copyCode(codeElementClone);
      }
    }
  });

  (function addLangLabel() {
    const blocks = codeBlocks();
    blocks.forEach(function(block){
      let label = block.dataset.lang;
      label = label === 'sh' ? 'bash' : label;
      if(label !== "fallback") {
        const labelEl = createEl();
        labelEl.textContent = label;
        pushClass(labelEl, 'lang');
        block.closest(`.${highlightWrap}`).appendChild(labelEl);
      }
    });
  })();
})();

;
(function calcNavHeight(){
  const nav = elem('.nav_header');
  const navHeight = nav.offsetHeight + 25;
  return navHeight;
})();

function toggleMenu(event) {
  const target = event.target;
  const isToggleControl = target.matches(`.${toggleId}`);
  const isWithToggleControl = target.closest(`.${toggleId}`);
  const showInstances = elems(`.${showId}`) ? Array.from(elems(`.${showId}`)) : [];
  const menuInstance = target.closest(`.${menu}`);

  function showOff(target, self = false) {
    showInstances.forEach(function(showInstance){
      if(!self) {
        deleteClass(showInstance, showId);
      }
      if(showInstance !== target.closest(`.${menu}`)) {
        deleteClass(showInstance, showId);
      }
    });
  }

  if(isToggleControl || isWithToggleControl) {
    const menu = isWithToggleControl ? isWithToggleControl.parentNode.parentNode : target.parentNode.parentNode;
    event.preventDefault();
    modifyClass(menu, showId);
  } else {
    if(!menuInstance) {
      showOff(target);
    } else {
      showOff(target, true);
    }
  }
}

(function markInlineCodeTags(){
  const codeBlocks = elems('code');
  if(codeBlocks) {
    codeBlocks.forEach(function(codeBlock){
      if(!hasClasses(codeBlock)) {
        codeBlock.children.length ? false : pushClass(codeBlock, 'noClass');
      }
    });
  }
})();

function activeHeading(position, listLinks) {
  let active = 'active';

  let linksToModify = Object.create(null);
  linksToModify.active = listLinks.filter(function(link) {
    return containsClass(link, active);
  })[0];

  // activeTocLink ? deleteClass

  linksToModify.new = listLinks.filter(function(link){
    return parseInt(link.dataset.position) === position
  })[0];

  if (linksToModify.active != linksToModify.new) {
    linksToModify.active ? deleteClass(linksToModify.active, active): false;
    pushClass(linksToModify.new, active);
  }
};

function loadActions() {
  (function updateDate() {
    const date = new Date();
    const year = date.getFullYear();
    const yearEl = elem('.year');
    yearEl ? year.innerHTML = year : false;
  })();

  (function customizeSidebar(){
    const tocActive = 'toc_active';
    const aside = elem('aside');
    const tocs = elems('nav', aside);
    if(tocs) {
      tocs.forEach(function(toc){
        toc.id = "";
        pushClass(toc, 'toc');
        if(toc.children.length >= 1) {
          const tocItems = Array.from(toc.children[0].children);

          const previousHeading = toc.previousElementSibling;
          previousHeading.matches('.active') ? pushClass(toc, tocActive) : false;

          tocItems.forEach(function(item){
            pushClass(item, 'toc_item');
            pushClass(item.firstElementChild, 'toc_link');
          })
        }
      });

      const currentToc = elem(`.${tocActive}`);

      if(currentToc) {
        const pageInternalLinks = Array.from(elems('a', currentToc));

        const pageIds = pageInternalLinks.map(function(link){
          return link.hash;
        });

        const linkPositions = pageIds.map(function(id){
          const heading = document.getElementById(id.replace('#',''));
          const position = heading.offsetTop;
          return position;
        });

        pageInternalLinks.forEach(function(link, index){
          link.dataset.position = linkPositions[index]
        });

        window.addEventListener('scroll', function(e) {
          // this.setTimeout(function(){
          let position = window.scrollY;
          let active = closestInt(position, linkPositions);
          activeHeading(active, pageInternalLinks);
          // }, 1500)
        });
      }
    }
  })();

  (function markExternalLinks(){
    let links = elems('a');
    const contentWrapperClass = '.content';
    if(links) {
      Array.from(links).forEach(function(link, index){
        let target, rel, blank, noopener, attr1, attr2, url, isExternal;
        url = elemAttribute(link, 'href');
        isExternal = (url && typeof url == 'string' && url.startsWith('http')) && !url.startsWith(parentURL) && link.closest(contentWrapperClass);
        if(isExternal) {
          target = 'target';
          rel = 'rel';
          blank = '_blank';
          noopener = 'noopener';
          attr1 = elemAttribute(link, target);
          attr2 = elemAttribute(link, noopener);

          attr1 ? false : elemAttribute(link, target, blank);
          attr2 ? false : elemAttribute(link, rel, noopener);
        }
      });
    }
  })();

  let headingNodes = [], results, link, icon, current, id,
  tags = ['h2', 'h3', 'h4', 'h5', 'h6'];

  current = document.URL;

  tags.forEach(function(tag){
    results = document.getElementsByTagName(tag);
    Array.prototype.push.apply(headingNodes, results);
  });

  function sanitizeURL(url) {
    // removes any existing id on url
    const hash = '#';
    const positionOfHash = url.indexOf(hash);
    if(positionOfHash > -1 ) {
      const id = url.substr(positionOfHash, url.length - 1);
      url = url.replace(id, '');
    }
    return url
  }

  headingNodes.forEach(function(node){
    link = createEl('a');
    icon = createEl('img');
    icon.src = 'https://passivepicasso.github.io/ThunderKit/docs/icons/link.svg';
    link.className = 'link icon';
    link.appendChild(icon);
    id = node.getAttribute('id');
    if(id) {
      link.href = `${sanitizeURL(current)}#${id}`;
      node.appendChild(link);
      pushClass(node, 'link_owner');
    }
  });

  function copyFeedback(parent) {
    const copyText = document.createElement('div');
    const yanked = 'link_yanked';
    copyText.classList.add(yanked);
    copyText.innerText = 'Link Copied';
    if(!elem(`.${yanked}`, parent)) {
      parent.appendChild(copyText);
      setTimeout(function() { 
        // parent.removeChild(copyText)
      }, 3000);
    }
  }

  (function copyHeadingLink() {
    let deeplink, deeplinks, newLink, parent, target;
    deeplink = 'link';
    deeplinks = elems(`.${deeplink}`);
    if(deeplinks) {
      document.addEventListener('click', function(event)
      {
        target = event.target;
        parent = target.parentNode;
        if (target && containsClass(target, deeplink) || containsClass(parent, deeplink)) {
          event.preventDefault();
          newLink = target.href != undefined ? target.href : target.parentNode.href;
          copyToClipboard(newLink);
          target.href != undefined ?  copyFeedback(target) : copyFeedback(target.parentNode);
        }
      });
    }
  })();

  const light = 'light';
  const dark = 'dark';
  const storageKey = 'colorMode';
  const key = '--color-mode';
  const data = 'data-mode';
  const bank = window.localStorage;

  function prefersColor(mode){
    return `(prefers-color-scheme: ${mode})`;
  }

  function systemMode() {
    if (window.matchMedia) {
      const prefers = prefersColor(dark);
      return window.matchMedia(prefers).matches ? dark : light;
    }
    return light;
  }

  function currentMode() {
    let acceptableChars = light + dark;
    acceptableChars = [...acceptableChars];
    let mode = getComputedStyle(doc).getPropertyValue(key).replace(/\"/g, '').trim();

    mode = [...mode].filter(function(letter){
      return acceptableChars.includes(letter);
    });

    return mode.join('');
  }

  /**
   * @param isDarkMode true means from dark to light, false means from light to dark
   */
  function changeMode(isDarkMode) {
    if(isDarkMode) {
      bank.setItem(storageKey, light)
      elemAttribute(doc, data, light);
    } else {
      bank.setItem(storageKey, dark);
      elemAttribute(doc, data, dark);
    }
  }

  (function lazy() {
    function lazyLoadMedia(element) {
      let mediaItems = elems(element);
      if(mediaItems) {
        Array.from(mediaItems).forEach(function(item) {
          item.loading = "lazy";
        });
      }
    }
    lazyLoadMedia('iframe');
    lazyLoadMedia('img');
  })();

  (function makeTablesResponsive(){
    const tables = elems('table');
    if (tables) {
      tables.forEach(function(table){
        const tableWrapper = createEl();
        pushClass(tableWrapper, 'scrollable');
        wrapEl(table, tableWrapper);
      });
    }
  })();

  function pickModePicture(user, system, context) {
    const pictures = elems('picture');
    if(pictures) {
      pictures.forEach(function(picture){
        let source = picture.firstElementChild;
        if(user == system) {
          context ? source.media = prefersColor(dark) : false;
        } else {
          if(system == light) {
            source.media = (user === dark) ? prefersColor(light) : prefersColor(dark) ;
          } else {
            source.media = (user === dark) ? prefersColor(dark) : prefersColor(light) ;
          }
        }
      });
    }
  }

  function setUserColorMode(mode = false) {
    const isDarkMode = currentMode() == dark;
    const storedMode = bank.getItem(storageKey);
    const sysMode = systemMode();
    if(storedMode) {
      if(mode) {
        changeMode(isDarkMode);
      } else {
        elemAttribute(doc, data, storedMode);
      }
    } else {
      if(mode === true) {
        changeMode(isDarkMode)
      } else {
        changeMode(sysMode!==dark);
      }
    }
    const userMode = doc.dataset.mode;
    doc.dataset.systemmode = sysMode;
    if(userMode) {
      pickModePicture(userMode,sysMode,mode);
    }
  }

  setUserColorMode();

  doc.addEventListener('click', function(event) {
    let target = event.target;
    let modeClass = 'color_choice';
    let isModeToggle = containsClass(target, modeClass);
    if(isModeToggle) {
      setUserColorMode(true);
    }
    toggleMenu(event);
  });

}

window.addEventListener('load', loadActions());