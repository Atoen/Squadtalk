let canvas;
let ctx;
const cellSize = 5;
let beatThreshold = 3;
let rows;
let columns;
let grid;
class Grid {
    cells;
    colorIndicesCache;
    width;
    height;
    ruleIndices;
    colors;
    constructor(width, height, colors) {
        this.width = width;
        this.height = height;
        this.colors = colors;
        this.colorIndicesCache = new Map();
        this.ruleIndices = new Map();
        this.cells = new Array(width);
        for (let i = 0; i < width; i++) {
            this.cells[i] = new Array(height);
            for (let j = 0; j < height; j++) {
                this.cells[i][j] = new Cell(i, j);
            }
        }
        this.CreateNeighbourLists();
    }
    CreateNeighbourLists() {
        for (let i = 0; i < this.width; i++)
            for (let j = 0; j < this.height; j++) {
                const cell = this.cells[i][j];
                for (let dx = -1; dx <= 1; dx++)
                    for (let dy = -1; dy <= 1; dy++) {
                        const x = i + dx;
                        const y = j + dy;
                        if (x < 0 || x >= rows ||
                            y < 0 || y >= columns ||
                            dx === 0 && dy === 0) {
                            continue;
                        }
                        cell.neighbours.push(this.cells[x][y]);
                    }
            }
    }
    FillCellsWithRandomColors() {
        for (let i = 0; i < this.width; i++) {
            for (let j = 0; j < this.height; j++) {
                const cell = this.cells[i][j];
                const color = this.colors[Math.floor(Math.random() * this.colors.length)];
                cell.SetColor(color);
            }
        }
    }
    GetCell(x, y) {
        return this.cells[x][y];
    }
    GetIndexOfColor(color) {
        if (this.colorIndicesCache.has(color)) {
            return this.colorIndicesCache.get(color);
        }
        for (let i = 0; i < this.colors.length; i++) {
            if (this.colors[i] === color) {
                this.colorIndicesCache.set(color, i);
                return i;
            }
        }
        return -1;
    }
    ClearRules() {
        this.ruleIndices.clear();
        this.colorIndicesCache.clear();
    }
}
class Cell {
    color;
    x;
    y;
    neighbours;
    constructor(x, y) {
        this.neighbours = [];
        this.x = x;
        this.y = y;
        this.color = "white";
    }
    SetColor(color) {
        this.color = color;
        this.Draw();
    }
    Draw() {
        const cellX = this.x * cellSize;
        const cellY = this.y * cellSize;
        ctx.fillStyle = this.color;
        ctx.fillRect(cellX, cellY, cellSize, cellSize);
    }
}
export function Init() {
    canvas = document.getElementById("canvas-simulation");
    ctx = canvas.getContext("2d");
    rows = Math.floor(canvas.height / cellSize);
    columns = Math.floor(canvas.width / cellSize);
    console.log(`Rows: ${rows}, Columns: ${columns}`);
}
function ArraysContainSameElements(arr1, arr2) {
    if (!arr2) {
        return false;
    }
    const uniqueArr1 = [...new Set(arr1)];
    const uniqueArr2 = [...new Set(arr2)];
    if (uniqueArr1.length !== uniqueArr2.length) {
        return false;
    }
    const sortedArr1 = uniqueArr1.sort();
    const sortedArr2 = uniqueArr2.sort();
    for (let i = 0; i < sortedArr1.length; i++) {
        if (sortedArr1[i] !== sortedArr2[i]) {
            return false;
        }
    }
    return true;
}
export function SetRules(rules, forceUpdate = false) {
    const colors = [...new Set(rules.map(x => x.a))];
    const theSame = ArraysContainSameElements(colors, grid?.colors);
    if (!theSame || forceUpdate) {
        grid = new Grid(rows, columns, colors);
        grid.FillCellsWithRandomColors();
    }
    else {
        grid.ClearRules();
        grid.colors = colors;
    }
    for (const rule of rules) {
        const attackerIndex = grid.GetIndexOfColor(rule.a);
        const attackedIndex = grid.GetIndexOfColor(rule.b);
        if (grid.ruleIndices.has(attackerIndex)) {
            grid.ruleIndices.get(attackerIndex).push(attackedIndex);
        }
        else {
            grid.ruleIndices.set(attackerIndex, [attackedIndex]);
        }
    }
    if (!theSame) {
        grid.FillCellsWithRandomColors();
    }
}
export function UpdateThreshold(threshold) {
    beatThreshold = threshold;
}
function getMaxIndex(numbers, excludeIndex) {
    let max = -Infinity;
    let index = -1;
    for (let i = 0; i < numbers.length; i++) {
        if (i === excludeIndex)
            continue;
        if (numbers[i] > max) {
            max = numbers[i];
            index = i;
        }
    }
    return index;
}
export function Step() {
    const colorsLength = grid.colors.length;
    const cellsToChangeColor = new Array(colorsLength);
    for (let i = 0; i < colorsLength; i++) {
        cellsToChangeColor[i] = [];
    }
    for (let i = 0; i < grid.width; i++) {
        for (let j = 0; j < grid.height; j++) {
            const cell = grid.GetCell(i, j);
            const cellColorIndex = grid.GetIndexOfColor(cell.color);
            const neighbours = cell.neighbours;
            const neighbourColors = new Array(colorsLength).fill(0);
            for (const neighbour of neighbours) {
                const index = grid.GetIndexOfColor(neighbour.color);
                neighbourColors[index]++;
            }
            const maxIndex = getMaxIndex(neighbourColors, cellColorIndex);
            if (neighbourColors[maxIndex] >= beatThreshold &&
                grid.ruleIndices.get(maxIndex).includes(cellColorIndex)) {
                cellsToChangeColor[maxIndex].push(cell);
            }
        }
    }
    for (let i = 0; i < colorsLength; i++) {
        for (let j = 0; j < cellsToChangeColor[i].length; j++) {
            const cell = cellsToChangeColor[i][j];
            cell.SetColor(grid.colors[i]);
        }
    }
}
//# sourceMappingURL=Simulation.js.map