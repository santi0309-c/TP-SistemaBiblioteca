import Data.Char (toUpper)

-- 1 a 
borrarUltimo :: [a] -> [a]
borrarUltimo [] = error "Lista Vacia"
borrarUltimo [x] = []
borrarUltimo (x:xs) = x : borrarUltimo xs

-- 1 b 
collect :: [(a, b)] -> [(a, [b])] 
collect [] = []
collect ((k, v):xs) = agregar k v (collect xs)
    where 
        agregar key val [] = [(key, [val])]
        agregar key val ((k', v') : resto)
         | key == k' = (k', val : v') : resto
         | otherwise = (k', v') : agregar key val resto
        
-- 2 c
divisores n = [x | x <- [2 .. (n - 1)], mod n x == 0]
abudantes = [n | n <- [1 ..], n < sum (divisores n)]

--3 a :t
--fuction :: (Int -> Int) -> Bool -> Bool
fuction f x = f 3 == 3 && x

-- 3 b
fuction2 :: Bool -> Int -> Bool
fuction2 c x = if c then x > 10 else x == 0  

-- 3 c 
fuction3 :: Char -> Char
fuction3 = toUpper 


-- 3 d
fuction4 :: Int -> (Int -> Bool) -> [Int]
fuction4 x f = if f x then [x] else []

-- 3 e
fuction5 ::  [a] -> (a -> [b]) -> [b]
fuction5  xs f = concat (map f xs)

-- 3 f

--fuction6 :: [[a]] -> (a -> Bool) -> [a]


-- 4 a
-- foo1 :: Bool -> .lBool -> Bool
foo1  p = if p then (p &&) else (p &&)

-- 4 b 
-- food2 :: (b -> c) -> (a -> b) -> a -> c 
foo2 x y z = x (y z)


max' :: [Int] -> Int
max' [] = error "Lista Vacia"
max' (x:xs) = foldr (\y acc -> if y < acc then acc  else y) x xs 



-- Practics 2 

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

--data Linea = L [Char] Int Deriving Show
type Cursor = Int 
type Linea = ([Char], Cursor)

-- ("hOLA", 3) --> ("HOL", "A")
type LineaPro = (String, String)





-- 3 
{-

headCl (Clist x) = x
headCl (Consnoc x xs z) = x

tailCl (CUnit x) = EmptyCl
tailCl (Consnoc x xs z) = Consnoc (headCl xs) (tailCl xs) z
tailCL (Consnoc x EmptyCl z) = CUnit z

rever EmptyCl = EmptyCl
rever (CUnit x) = CUnit x
rever (Consnoc x xs z) = Consnoc z (rev xs) x

-- 6 


completo 0 x = E 
completo n x = let t = (completo (n - 1) x)
                    in N t x t

--completo n x = N (completo (n - 1) x) x (completo (n - 1) x)


balanceado 0 x = E
balanceado n x  | odd n = let l = (div (n - 1) 2)
                             t = (balanceado l x)
                          in N t x t 
                | otherwise = let (t1, t2) = ((balanceado (div n 2)), (balanceado (div (n - 1) 2)))
                              in N t1 x t2


balanceado 0 x = E 
balanceado n x | odd n = N t x t
               | otherwise = N t x t2
               where 
                    m = div (n - 1) 2
                    t = (balanceado m x)
                    t2 = (balnceado (div (n - 1) 2)) 

1 2 3 
  1
2   3

1 2 3 4
   1
2     3
    4

-}