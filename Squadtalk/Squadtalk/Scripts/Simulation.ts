let canvas: HTMLCanvasElement;
let ctx: CanvasRenderingContext2D;

const cellSize = 5;
const beatThreshold = 2;

let rows: number;
let columns: number;

let grid: Grid;

type Color = "red" | "yellow" | "green" | "white";

class Grid {
    private readonly cells: Cell[][];
    private readonly colorIndicesCache: Map<Color, number>;
    
    readonly width: number;
    readonly height: number;
    
    readonly colors: Color[];
    rules: Map<number, number>;
    
    constructor(width: number, height: number, colors: Color[]) {
        this.width = width;
        this.height = height;
        this.colors = colors;
        
        this.colorIndicesCache = new Map<Color, number>();
        this.rules = new Map<number, number>();
        
        this.cells = new Array<Cell[]>(width);        
        
        for (let i = 0; i < width; i++) {
            this.cells[i] = new Array<Cell>(height);
            for (let j = 0; j < height; j++) {
                this.cells[i][j] = new Cell(i, j);
            }
        }
        
        this.CreateNeighbourLists();
    }
    
    CreateNeighbourLists(): void {
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
    
    FillCellsWithRandomColors(): void {
        for (let i = 0; i < this.width; i++) {
            for (let j = 0; j < this.height; j++) {
                const cell = this.cells[i][j];
                const color =this.colors[Math.floor(Math.random() * this.colors.length)];
                cell.SetColor(color);
            }
        }
    }
    
    GetCell(x: number, y: number): Cell {
        return this.cells[x][y];
    }
    
    GetIndexOfColor(color: Color): number {
        if (this.colorIndicesCache.has(color)) {
            return this.colorIndicesCache.get(color)!;
        }        

        for (let i = 0; i < this.colors.length; i++) {
            if (this.colors[i] === color) {
                this.colorIndicesCache.set(color, i);
                return i;
            }
        }
        
        return -1;
    }
}

class Cell {
    color: Color;
    x: number;
    y: number;
    
    readonly neighbours: Cell[]
    
    constructor(x: number, y: number) {
        this.neighbours = [];
        this.x = x;
        this.y = y;
        this.color = "white";
    }
    
    SetColor(color: Color): void {
        this.color = color;
        this.Draw();
    }
    
    Draw(): void {
        const cellX = this.x * cellSize;
        const cellY = this.y * cellSize;

        ctx.fillStyle = this.color;
        ctx.fillRect(cellX, cellY, cellSize, cellSize);
    }
}

export function Init() {
    canvas = document.getElementById("canvas-simulation") as HTMLCanvasElement;
    ctx = canvas.getContext("2d");

    rows = Math.floor(canvas.height / cellSize);
    columns = Math.floor(canvas.width / cellSize);
    
    grid = new Grid(rows, columns, ["red", "green", "yellow", "white"]);
    grid.FillCellsWithRandomColors();

    grid.rules.set(0, 1);
    grid.rules.set(1, 2);
    grid.rules.set(2, 3);
    grid.rules.set(3, 0);
}

function getMaxIndex(numbers: number[], excludeIndex: number): number {
    let max = -Infinity;
    let index = -1;

    for (let i = 0; i < numbers.length; i++) {
        if (i === excludeIndex) continue;

        if (numbers[i] > max) {
            max = numbers[i];
            index = i;
        }
    }

    return index;
}

export function Step() {
    const colorsLength = grid.colors.length;
    
    const cellsToChangeColor: Cell[][] = new Array<Cell[]>(colorsLength);
    for (let i = 0; i < colorsLength; i++) {
        cellsToChangeColor[i] = [];
    }
    
    for (let i = 0; i < grid.width; i++) {
        for (let j = 0; j < grid.height; j++) {
            const cell = grid.GetCell(i, j);
            const cellColorIndex= grid.GetIndexOfColor(cell.color);
            
            const neighbours = cell.neighbours;
            const neighbourColors = new Array<number>(colorsLength).fill(0);
            
            for (const neighbour of neighbours) {
                const index = grid.GetIndexOfColor(neighbour.color);
                neighbourColors[index]++;
            }
            
            const maxIndex = getMaxIndex(neighbourColors, cellColorIndex);
            if (neighbourColors[maxIndex] >= beatThreshold && grid.rules.get(maxIndex) === cellColorIndex) {
                cellsToChangeColor[maxIndex].push(cell);
            }
        }
    }

    for (let i = 0; i < colorsLength; i++) {
        for (let j = 0; j < cellsToChangeColor[i].length; j++) {
            const cell = cellsToChangeColor[i][j];
            cell.SetColor(grid.colors[i])
        }
    }
}
