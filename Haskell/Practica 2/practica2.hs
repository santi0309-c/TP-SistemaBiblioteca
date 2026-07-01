import Data.Sequence (Seq(Empty))
import Foreign.C (e2BIG)
--1

type Color1 = (Int, Int, Int)

mezclar1 :: Color1 -> Color1 -> Color1
mezclar1 (r1, g1, b1) (r2, g2, b2) = 
    ((r1 + r2) `div` 2, (g1 + g2) `div` 2, (b1 + b2) `div` 2)


data Color2 = C Int Int Int deriving Show

mezclar2 :: Color2 -> Color2 -> Color2
mezclar2 (C r1 g1 b1) (C r2 g2 b2) = 
    C ((r1 + r2) `div` 2) ((g1 + g2) `div` 2) ((b1 + b2) `div` 2)

data CRGB = Negro | Blanco | Gris Int | RGB (Int, Int, Int) deriving Show

-- Función auxiliar para pasar cualquier CRGB a (Int, Int, Int)
toRGB :: CRGB -> (Int, Int, Int)
toRGB Negro          = (0, 0, 0)
toRGB Blanco         = (255, 255, 255)
toRGB (Gris n)       = (n, n, n)
toRGB (RGB (r,g,b))  = (r, g, b)

mezclar3 :: CRGB -> CRGB -> CRGB
mezclar3 c1 c2 = RGB ((r1+r2) `div` 2, (g1+g2) `div` 2, (b1+b2) `div` 2)
  where 
    (r1, g1, b1) = toRGB c1
    (r2, g2, b2) = toRGB c2


--Strictness (!): El símbolo ! obliga a Haskell a evaluar los números inmediatamente, evitando la acumulación de "thunks" (operaciones pendientes en memoria) que es el problema #1 de rendimiento en Haskell.

--Legibilidad: Al usar nombres (r, g, b), el código es menos propenso a errores de posición que las tuplas.

--Acceso directo: Los Records generan funcion



-- propongan un tipo de dato dnd la mezcla sea mas eficiente
data ColorE = Color { r :: !Int, g :: !Int, b :: !Int } deriving Show

mezclarE :: ColorE -> ColorE -> ColorE
mezclarE (Color r1 g1 b1) (Color r2 g2 b2) = 
    Color ((r1 + r2) `div` 2) ((g1 + g2) `div` 2) ((b1 + b2) `div` 2)

data ColorR = ColorR { 
    red   :: Int, 
    green :: Int, 
    blue  :: Int 
} deriving (Show)

mezclarR :: ColorR -> ColorR -> ColorR
mezclarR c1 c2 = ColorR {
    red  = (red c1 + red c2) `div` 2,
    green = (green c1 + green c2) `div` 2,
    blue  = (blue c1 + blue c2) `div` 2
}


-- 2
--data Linea = L [Char] Int Deriving Show
type Cursor = Int 
type Linea = ([Char], Cursor) Deriving Show

vacia' :: Linea  
vacia' = ([], 0)

moverIzq' ::  Linea -> Linea
moverIzq' (xs, n) 
                | n /= 0 = (xs, n - 1)
                | otherwise = (xs, 0)
                

moverDer' :: Linea -> Linea
moverDer' (xs, n) 
                | length xs > n = (xs, n + 1)
                | otherwise = (xs, n)

moverIni' :: Linea -> Linea 
moverIni' (xs, _) = (xs, 0)

moverFin' :: Linea -> Linea 
moverFin' (xs, _) = (xs, length xs)

insertar' :: Char -> Linea -> Linea
insertar' c (xs, n) = (insertarAux c n xs, n + 1)
    where 
        insertarAux :: Char -> Int -> [Char] -> [Char]
        insertarAux c 0 xs = c:xs
        insertarAux c n (x:xs) = x : insertarAux c (n - 1) xs

borrar' :: Linea -> Linea
borrar' (xs,n) = (borrarAux xs, n)
    where
        borrarAux :: Linea -> [Char]
        borrarAux (x:xs, 0) = xs
        borrarAux (x:xs, n) = x : borrarAux (xs, n - 1)

-- ("hOLA", 3) --> ("HOL", "A")
type LineaPro = (String, String)

-- (1) La constante vacía
vacia :: LineaPro
vacia = ("", "")

-- (2) Mover a la izquierda
moverIzq :: LineaPro -> LineaPro
moverIzq (c:izq, der) = (izq, c:der)
moverIzq ("", der)     = ("", der) -- No hay nada a la izquierda

-- (3) Mover a la derecha
moverDer :: LineaPro -> LineaPro
moverDer (izq, c:der) = (c:izq, der)
moverDer (izq, "")     = (izq, "") -- No hay nada a la derecha

-- (4) Mover al comienzo
moverIni :: LineaPro -> LineaPro
moverIni (izq, der) = ("", reverse izq ++ der)

-- (5) Mover al final
moverFin :: LineaPro -> LineaPro
moverFin (izq, der) = (reverse der ++ izq, "")

-- (6) Borrar el carácter a la izquierda del cursor
borrar :: LineaPro -> LineaPro
borrar (_:izq, der) = (izq, der)
borrar ("", der)    = ("", der)

-- (7) Insertar y mover a la derecha
insertar :: Char -> LineaPro -> LineaPro
insertar c (izq, der) = (c:izq, der)


-- 3 

data CList a = EmptyCL | CUnit a | Consnoc a (CList a) a 

-- a

isEmptyCL :: CList a -> Bool
isEmptyCL EmptyCL = True
isEmptyCL _       = False

isCUnit :: CList a -> Bool
isCUnit (CUnit _) = True
isCUnit _         = False

headCL :: CList a -> a
headCL (CUnit x)        = x
headCL (Consnoc l _ _)  = l
headCL EmptyCL          = error "headCL: La lista está vacía"

tailCL :: CList a -> a
tailCL (CUnit x)        = x
tailCL (Consnoc _ _ l)  = l
tailCL EmptyCL          = error "tailCL: La lista está vacía"


-- b
reverseCL :: CList a -> CList a
reverseCL (CUnit x)        = CUnit x
reverseCL (Consnoc l c r)  = Consnoc l reverseCL c r
reverseCL EmptyCL          = EmptyCL 


-- c
-- Agrega un elemento al "comienzo" (izquierda) de la CList
consCL :: a -> CList a -> CList a
consCL x EmptyCL         = CUnit x
consCL x (CUnit y)       = Consnoc x EmptyCL y
consCL x (Consnoc l c r) = Consnoc x (consCL l c) r

-- Aplica una función a cada elemento de la CList
mapCL :: (a -> b) -> CList a -> CList b
mapCL _ EmptyCL         = EmptyCL
mapCL f (CUnit x)       = CUnit (f x)
mapCL f (Consnoc l c r) = Consnoc (f l) (mapCL f c) (f r)

inits :: CList a -> CList (CList a)
inits EmptyCL = CUnit EmptyCL
inits xs = consCL EmptyCL (mapCL (consCL (headCL xs)) (inits (tailCL xs)))


-- d
lasts :: CList a -> CList (CList a)
lasts EmptyCL = CUnit EmptyCL
lasts xs = consCL xs (lasts (tailCL xs))


-- e
-- Agrega un elemento a la derecha (Snoc)
snocCL :: CList a -> a -> CList a
snocCL EmptyCL x         = CUnit x
snocCL (CUnit x) y       = Consnoc x EmptyCL y
snocCL (Consnoc l c r) x = Consnoc l (snocCL c r) x

-- Une dos CList en una sola
appendCL :: CList a -> CList a -> CList a
appendCL EmptyCL ys         = ys
appendCL (CUnit x) ys       = consCL x ys
appendCL (Consnoc l c r) ys = consCL l (appendCL (snocCL c r) ys)


concatCL :: CList (CList a) -> CList a
concatCL EmptyCL = EmptyCL
concatCL (CUnit xs) = xs
concatCL (Consnoc l c r) = appendCL l (appendCL (concatCL c) r)


-- 4
data Aexp = Num Int | Prod Aexp Aexp | Div Aexp Aexp

-- a
eval :: Aexp -> Int
eval Num n = n 
eval Prod e1 e2 = eval e1 * eval e2
eval Div e1 e2 = eval e1 `div` (if eval e2 /= 0 then eval e2 else error "div por 0")


-- b 
seval :: Aexp -> Maybe Int
seval (Num n) = Just n
seval (Prod e1 e2) = combinarProd (seval e1) (seval e2)
seval (Div e1 e2) = combinarDiv (seval e1) (seval e2)


combinarProd :: Maybe Int -> Maybe Int -> Maybe Int
combinarProd (Just v1) (Just v2) = Just (v1 * v2)
combinarProd _         _         = Nothing

combinarDiv :: Maybe Int -> Maybe Int -> Maybe Int
combinarDiv (Just v1) (Just v2) | v2 /= 0    = Just (v1 `div` v2)
                                | otherwise  = Nothing
combinarDiv _         _         = Nothing


-- 5 
data BST a = Hoja | Nodo (BST a) a (BST a)

-- a 
maximum' :: BST a -> a 
maximum' Hoja            = error "El Arbol esta vacio"
maximum' (Nodo _ x Hoja) = x
maximum' (Nodo _ _ der)  = maximum' der 


-- b

checkBST :: BST a -> Bool
checkBST arbol = esCreciente (inOrder arbol)


inOrder :: BST a -> [a]
inOrder Hoja             = []
inOrder (Nodo izq x der) = inOrder izq ++ [x] ++ inOrder der

esCreciente :: [a] -> Bool
esCreciente []       = True 
esCreciente [_]      = True
esCreciente (x:y:xs) = x < y && esCreciente (y:xs)


-- 6 
data Tree a = EmptyT | Node (Tree a) a (Tree a) deriving Show

-- a
completo :: a -> Int -> Tree a
completo x 0 = EmptyT
completo x d = 
    let subArbol = completo x (d - 1)
    in Node subArbol x subArbol


-- b 
balanceado :: a -> Int -> Tree as
balanceado x 0 = EmptyT
balanceado x n =
    let (q, r) = (n - 1) `divMod` 2 
        sub1   = balanceado x q
    in if r == 0 
       then Node sub1 x sub1
       else let sub2 = balanceado x (q + 1)
            in  Node sub1 x sub2 


-- 7
member :: Ord a => a -> BST a -> Bool
member x Hoja = False
member x (Nodo izq v der) = aux x v (Nodo izq v der)
  where
    aux x c Hoja = x == c
    aux x c (Nodo izq v der)
        | x <= v    = aux x v izq  -- Si x <= v, v es el nuevo candidato a ser igual a x
        | otherwise = aux x c der  -- Si x > v, mantenemos el candidato anterior


-- 8
data Color = R | B
data RBT a = EmptyR | Node' Color (RBT a) a (RBT a)


fromOrdList :: [a] -> RBT a
fromOrdList xs = build (length xs) xs
  where
    build 0 ys = (EmptyR, ys)
    build n ys =
        let sizeIzq = n `div` 2
            (izq, x:resto) = build sizeIzq ys
            sizeDer = n - sizeIzq - 1
            (der, final) = build sizeDer resto
        in (Node B izq x der, final)


-- Función principal que extrae el árbol de la tupla
buildRBT :: [a] -> RBT a
buildRBT xs = fst (build (length xs) xs)


-- 9 
lbalance :: Color -> RBT a -> a -> RBT a -> RBT a
-- Caso 1: El hijo izquierdo es rojo y su hijo izquierdo también
lbalance B (Node R (Node R a x b) y c) z d = Node R (Node B a x b) y (Node B c z d)
-- Caso 2: El hijo izquierdo es rojo y su hijo derecho también
lbalance B (Node R a x (Node R b y c)) z d = Node R (Node B a x b) y (Node B c z d)
-- Si no hay violación por la izquierda, se mantiene igual
lbalance color l v r = Node color l v r


rbalance :: Color -> RBT a -> a -> RBT a -> RBT a
-- Caso 3: El hijo derecho es rojo y su hijo izquierdo es rojo
rbalance B a x (Node R (Node R b y c) z d) = Node R (Node B a x b) y (Node B c z d)
-- Caso 4: El hijo derecho es rojo y su hijo derecho también
rbalance B a x (Node R b y (Node R c z d)) = Node R (Node B a x b) y (Node B c z d)
-- Si no hay violación por la derecha, se mantiene igual
rbalance color l v r = Node color l v r


-- b

insert :: Ord a => a -> RBT a -> RBT a
insert x t = makeBlack (ins t)
  where
    makeBlack (Node _ l v r) = Node B l v r
    makeBlack EmptyR          = EmptyR

    ins EmptyR = Node R EmptyR x EmptyR
    ins (Node color l v r)
        | x < v     = lbalance color (ins l) v r  -- Insertamos en izq, usamos lbalance
        | x > v     = rbalance color l v (ins r)  -- Insertamos en der, usamos rbalance
        | otherwise = Node color l v r


-- 10
type Rank = Int
data Heap a = EmptyH | NodeH Rank a (Heap a) (Heap a) deriving (Show)

fromList :: Ord a => [a] -> Heap a
fromList [] = EmptyH
fromList xs = mergeAll (map singleton xs)
  where
    -- singleton crea un heap de un solo elemento (rango 1)
    singleton x = NodeH 1 x EmptyH EmptyH


    merge :: Ord a => Heap a -> Heap a -> Heap a
    merge h1 EmptyH = h1
    merge EmptyH h2 = h2
    merge h1@(NodeH _ x l1 r1) h2@(NodeH _ y l2 r2)
        | x <= y    = makeNode x l1 (merge r1 h2)
        | otherwise = makeNode y l2 (merge h1 r2)

    -- Función auxiliar para mantener la propiedad leftist
    makeNode :: a -> Heap a -> Heap a -> Heap a
    makeNode x l r
        | rank l >= rank r = NodeH (rank r + 1) x l r
        | otherwise        = NodeH (rank l + 1) x r l
        where 
            rank EmptyH = 0
            rank (NodeH r _ _ _) = r


    -- mergePairs recorre la lista mezclando de a dos
    mergePairs (h1:h2:rest) = merge h1 h2 : mergePairs rest
    mergePairs hs           = hs

    -- mergeAll combina los heaps en pares hasta que queda uno solo
    mergeAll [h] = h
    mergeAll hs  = mergeAll (mergePairs hs)

