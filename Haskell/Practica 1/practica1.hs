import Data.Char (toUpper)

-- 1 a 
borrarUltimo :: [a] -> [a]
borrarUltimo [] = error "Lista Vacia"
borrarUltimo [x] = []
borrarUltimo (x:xs) = x : borrarUltimo xs

-- 1 b 
collect :: Eq a => [(a, b)] -> [(a, [b])] 
collect [] = []
collect ((k, v):xs) = agregar' k v (collect xs)
    where  
        agregar' key val [] = [(key, [val])]
        agregar' key val ((k', v') : resto) 
         | key == k' = (k', val : v') : resto
         | otherwise = (k', v') : agregar' key val resto

-- 1 c
serie :: [Int] -> [[Int]]
serie [] = [[]]
serie xs = serie (init xs) ++ [xs]

-- 1 d
paresIguales :: Int -> Int -> Int -> Int -> Bool
paresIguales a b c d = com1 || com2 || com3
    where 
        com1 = a == b && c == d
        com2 = a == c && b == d
        com3 = a == d && b == c

--1 e
isoceles :: Int -> Int ->  Int -> Bool
isoceles a b c = a == b || a == c || b == c

-- 1 f
ror :: [a] -> Int -> [a]
ror [] _ = []
ror xs 0 = xs
ror (x:xs) n = ror (xs ++ [x]) (n - 1) 
   
ror' :: [a] -> Int -> [a]
ror' [] _ = []
ror' xs 0 = xs
ror' xs n = ror (rotarUnaVez xs) (n - 1)
  where
    -- Mueve el último elemento al principio de la lista
    rotarUnaVez :: [a] -> [a]
    rotarUnaVez ys = ponerAlPrincipio (ultimo ys) (borrarUltimo ys)

    -- Busca el último elemento recursivamente
    ultimo [x]    = x
    ultimo (x:xs) = ultimo xs

    -- Devuelve la lista sin el último elemento recursivamente
    borrarUltimo [x]    = []
    borrarUltimo (x:xs) = x : borrarUltimo xs

    -- Función auxiliar para unir el elemento con el resto
    ponerAlPrincipio e resto = e : resto

{-- 1 g
upto :: Int -> Int -> [Int]
upto n m
        | n == m = []
        | (n + 1) == m = [m]
        | z == n = 
        | n < m = (n + 1) : upto (n + 1) m
    where 
        z = n-}

upto :: Int -> Int -> [Int]
upto n m
    | n == m = []
    | (n + 1) == m = [n, m]
    | n < m  = n : upto (n + 1) m


count :: Int -> Int -> Int
count n m 
    | n == m = 0
    | n < m = 1 + count (n + 1) m 


-- 1 h
eco :: String -> String
eco [] = []
eco xs = [x | (x, i) <- zip xs [1..], _ <- [1..i]]



-- 2 a
cambios :: Eq a => [a] -> [Int]
cambios [] = []
cambios xs = [i | (a, b, i) <- zip3 xs (tail xs) [1..], a /= b] 


-- 2 b
oblongoNumber :: [Int]
oblongoNumber = [x * y | (x, y) <- zip [1..7] [2..8] ]


-- 2 c
divisores n = [x | x <- [2 .. (n - 1)], mod n x == 0]
abudantes = [n | n <- [1 ..], n < sum (divisores n)]


-- 2 d
eco' :: String -> String
eco' [] = []
eco' xs = dupAux xs 1

dupAux :: [a] -> Int -> [a]
dupAux [] _ = []
dupAux (x:xs) n = replicate x n ++ dupAux xs (n + 1)
    where
        replicate _ 0 = []
        replicate x n = x : replicate x (n - 1) 

-- 2 e
euler :: Int -> Int
euler 0 = 0
euler n = foldr (+) 0 (mul3 n) + foldr (+) 0 (mul5 n)
    where
        mul3 :: Int -> [Int]
        mul3 n = filter (< n) m3
            where 
                m3 = [x * 3 | x  <- [0..n]]

        mul5 :: Int -> [Int]
        mul5 n = filter (< n) m5
            where 
                m5 = [x * 5 | x  <- [0..n]]

-- 2 f
expandir :: [Int] -> [Int]
expandir [] = []
expandir [0] = [0]
expandir (x:xs) = expandirAux x x ++ expandir xs
        where
            expandirAux x 0 = []
            expandirAux x n = x : expandirAux x (n - 1)


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

fuction6 :: [[a]] -> (a -> Bool) -> [a]
fuction6 (xs:xss) f = filter f xs ++ fuction6 xss f

-- 3 g
fuction7 :: Eq a => (a, a, a) -> Bool
fuction7 (x, y, z)
                | x == y || x == z = True
                | y == z           = True
                | otherwise        = False

-- 3 h 
fuction8 :: (a, b, c) -> Int -> c
fuction8 (_, _, z) _ = z


-- 3 i
fuction9 :: (a, a, a) -> Int -> a
fuction9 (x, _, _) 0 = x
fuction9 (_, y, _) 1 = y
fuction9 (_, _, z) _ = z


-- 4 a
-- foo1 :: Bool -> .lBool -> Bool
foo1  p = if p then (p &&) else (p &&)

-- 4 b 
-- food2 :: (b -> c) -> (a -> b) -> a -> c 
foo2 x y z = x (y z)


max' :: [Int] -> Int
max' [] = error "Lista Vacia"
max' (x:xs) = foldr (\y acc -> if y < acc then acc  else y) x xs 


-- 4 c 
foo3 :: (a -> b -> c) -> a -> b -> c
foo3 x y z = x y z


-- 4 d
foo4 :: (a -> b) -> a -> [b] -> [b]
foo4 x y z = x y : z


-- 4 e 
foo5 :: a -> ([a] -> [a]) -> [a] -> [a]
foo5 x y z = x : y z


-- 4  f
foo6 :: [a] -> ([a] -> [a]) -> [a] -> [a]
foo6 x y z = x ++ y z


-- 4 g
foo7 :: [[a]] -> ([[a]] -> Bool) -> [a]
foo7 a b = if b a then head a else []


-- 4 h
foo8 :: [a] -> ([a] -> Bool) -> [a]
foo8 a b = if b a then a else []


-- 4 i
--foo9 ::  [a] -> ([a] -> Bool) -> [a]
--foo9 :: [a1] -> ([a1] -> Bool) -> a2 -> [a2]
--foo9 a b = if b a then head (: a) else (: [])


--5 a

map' :: (a -> b) -> [a] -> [b]
map' f = foldr (\ x acc -> f x : acc) []
 

-- 5 b 
filter' :: (a -> Bool) -> [a] -> [a]
filter' f = foldr (\ x acc -> if f x then x : acc else acc) []


-- 5 c 
unzip :: [(a,b)] -> ([a], [b])
unzip = foldr (\(x, y) (xs, ys) -> (x:xs, y:ys)) ([], [])

-- 5 d
pair2List :: (a, [b]) -> [(a,b)]
pair2List (x, xs) = foldr (\y acc -> (x,y) : acc) [] xs

-- 5 e
maxL :: (Int, Int) -> (Int, Int) -> (Int, Int)
maxL (a, b) (c, d) | (b - a) >= (d - c) = (a, b)
                   | otherwise          = (c, d)

maxSec :: [(Int, Int)] -> (Int, Int)
maxSec [] = error "Lista vacía"
maxSec (s:ss) = foldr maxL s ss