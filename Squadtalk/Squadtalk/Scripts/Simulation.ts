let canvas: HTMLCanvasElement;
let ctx: CanvasRenderingContext2D;

const cellSize = 5;
let rows: number;
let columns: number;

type Color = "red" | "yellow" | "green" | "white";
type Cell = {
    color: Color;
    x: number;
    y: number;
};

let grid: Cell[][];
let neighbourList: Cell[][][];

export function Init() {
    running = false;
    canvas = document.getElementById("canvas-simulation") as HTMLCanvasElement;
    ctx = canvas.getContext("2d");

    rows = Math.floor(canvas.height / cellSize);
    columns = Math.floor(canvas.width / cellSize);

    neighbourList = [];
    grid = new Array(rows);
    for (let i = 0; i < rows; i++) {
        grid[i] = new Array(columns);
        neighbourList[i] = [];
        for (let j = 0; j < columns; j++) {
            grid[i][j] = { color: "white", x: i, y: j };
            neighbourList[i][j] = [];
        }
    }

    // DrawGrid();
    FillStartingCells();
    GenerateNeighbourList();
}

let running = false;
export function Start() {
    if (running) return;
    running = true;
    setInterval(SimulationStep, 200);
}

// function checkNeighbour(x: number, y: number) {
//    
//     const color = grid[x][y].color;
//     if (color === "empty") return;
//    
//     for (let i = -1; i <= 1; i++) {
//         for (let j = -1; j <= 1; j++) {
//            
//             const nX = x + j;
//             const nY = y + i;
//            
//             if (nX < 0 || nX >= rows ||
//                 nY < 0 || nY >= columns ||
//                 i === 0 && j === 0) continue;
//            
//             const neighbour = grid[nX][nY];
//             if (Beats(color, neighbour.color)) {
//                 neighbour.color = color;
//                 FillCell(nX, nY, color);
//                
//                 return;
//             }
//         }
//     }
// }

function SimulationStep() {

    let to_death: Cell[] = [];
    let to_birth: Cell[] = [];
    
    for (let i = 0; i < rows; i++) {
        for (let j = 0; j < columns; j++) {
            const cell = grid[i][j];
            const neighbours = neighbourList[i][j];
            const neighboursAlive = neighbours.filter(x => x.color === "red").length;
            if (cell.color === "white") {
                if (neighboursAlive == 3) {
                    to_birth.push(cell);
                }
            }
            if (neighboursAlive <= 1 || neighboursAlive >= 4) {
                to_death.push(cell);
            }
        }
    }

    for (const cell of to_birth) {
        cell.color = "red"
        FillCell(cell);
    }

    for (const cell of to_death) {
        cell.color = "white";
        FillCell(cell);
    }
}

function GenerateNeighbourList(){
    for (let i = 0; i < rows; i++)
    for (let j = 0; j < columns; j++) {
        
        let neighbours: Cell[] = [];
        
        for (let dx = -1; dx <= 1; dx++)
        for (let dy = -1; dy <= 1; dy++) {
            const nX = i + dx;
            const nY = j + dy;
            if (nX < 0 || nX >= rows ||
                nY < 0 || nY >= columns ||
                dx === 0 && dy === 0) {
                continue;
            }
            
            neighbours.push(grid[nX][nY])
        }
        
        neighbourList[i][j] = neighbours;
    }
}

// function FillStartingCells() {
//     const colors: Color[] = ["red", "yellow", "green"];
//
//     function getRandomInt(min: number, max: number) {
//         return Math.floor(Math.random() * (max - min + 1)) + min;
//     }
//
//     for (let i = 0; i < rows; i++) {
//         for (let j = 0; j < columns; j++) {
//             let color = colors[getRandomInt(0, colors.length - 1)];
//            
//             grid[i][j].color = color;
//             FillCell(i, j, color);
//         }
//     }
// }

function FillStartingCells() {
    const colors: Color[] = ["red"];

    function getRandomInt(min: number, max: number): number {
        return Math.floor(Math.random() * (max - min + 1)) + min;
    }

    function isCellOccupied(row: number, column: number): boolean {
        return grid[row][column].color !== "white";
    }

    for (const color of colors) {
        for (let i = 0; i < 800; i++) {
            let row: number, column: number;
            do {
                row = getRandomInt(0, rows - 1);
                column = getRandomInt(0, columns - 1);
            } while (isCellOccupied(row, column));

            grid[row][column].color = color;
            FillCell(grid[row][column]);
        }
    }
}

function Beats(color: Color, adjacentColor: Color): boolean {
    
    if (adjacentColor === "white") return true;
    if (color === "white") return false;
    
    return color === "red" && adjacentColor === "green" ||
        color === "green" && adjacentColor === "yellow" ||
        color === "yellow" && adjacentColor === "red"
}

function DrawGrid() {
    rows = Math.floor(canvas.height / cellSize);
    columns = Math.floor(canvas.width / cellSize);

    for (let i = 0; i <= rows; i++) {
        ctx.beginPath();
        ctx.moveTo(0, i * cellSize);
        ctx.lineTo(canvas.width, i * cellSize);
        ctx.stroke();
    }

    for (let j = 0; j <= columns; j++) {
        ctx.beginPath();
        ctx.moveTo(j * cellSize, 0);
        ctx.lineTo(j * cellSize, canvas.height);
        ctx.stroke();
    }
}

export function FillCell(cell: Cell) {
    const cellX = cell.x * cellSize;
    const cellY = cell.y * cellSize;

    ctx.fillStyle = cell.color;
    ctx.fillRect(cellX, cellY, cellSize, cellSize);
}