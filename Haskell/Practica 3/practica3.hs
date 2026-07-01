-- 1
data Nat = Cero | Succ Nat

-- a
{-
El constructor Succ es una función que toma un número natural y devuelve otro número natural (el siguiente).

Tipo: Succ :: Nat -> Nat
-}


-- b
int2Nat :: Int -> Nat 
int2Nat 0 = Cero
int2Nat n = Succ(int2Nat (n - 1))


-- c
sumaNat :: Nat -> Nat -> Nat
sumaNat Cero n = n
sumaNat (Succ n) m = Succ (sumaNat n m)

-- d
nat2Int :: Nat -> Int
nat2Int Cero = 0
nat2Int (Succ n) = 1 + nat2Int n


-- 2
data Arb = E | H Int | N Arb Arb

data Cmd = L | R


-- a
{-
El constructor N toma dos árboles (el subárbol izquierdo y el derecho) y produce un nuevo árbol combinado.
Tipo: N :: Arb -> Arb -> Arb
-}


-- b 
select :: [Cmd] -> Arb -> Arb
select [] a = a
select (L:xs) (N izq der) = select xs izq
select (R:xs) (N izq der) = select xs der


-- c
enum :: Arb -> [[Cmd]]
enum E = []
enum (H _) = [[]]
enum (N izq der) = map (L:) (enum izq) ++ map (R:) (enum der)



-- 3 
type Nombre = String
type Estado a = [(Nombre, a)]


inicial :: Estado a
inicial = []


update :: Nombre -> a -> Estado a -> Estado a
update n v [] = [(n, v)]
update n v ((nom,val):xs)   
    | n == nom = (n, val):xs
    | otherwise = (nom, val) : update n v xs


lookfor :: Nombre -> Estado a -> a
lookfor nom [] = error "No existe un Nombre en el Estado"
lookfor nom ((n, v):xs)
    | nom == n  = v
    | otherwise = lookfor nom xs


free :: Eq a => a -> Estado a -> Estado a
free val [] = []
free val ((n, v):xs) 
    | val == v  = xs
    | otherwise = (n, v): free val xs



-- 4 

-- a
data Arb' a = E' | N' (Arb' a) a (Arb' a) deriving (Show, Eq)

nivel :: Int -> Arb' a -> Int 
nivel _ E' = 0
nivel 0 _ = 1
nivel n (N' izq _ der) = nivel (n - 1) izq + nivel (n - 1) der


-- b 
altura :: Arb' a -> Int
altura E' = 0
altura (N' i _ d) = 1 + max (altura i) (altura d)

esBalanceado :: Arb' a -> Bool
esBalanceado E' = True
esBalanceado (N' i _ d) = abs (altura i - altura d) <= 1 && esBalanceado i && esBalanceado d


-- c
-- Devuelve (Predecesor, Sucesor)
sucPre :: Ord a => a -> Arb' a -> (Maybe a, Maybe a)
sucPre x t = go t Nothing Nothing
  where
    go E' p s = (p, s)
    go (N' i v d) p s
        | x == v = (findMax i, findMin d)
        | x < v  = go i p (Just v)  -- El actual es un posible sucesor
        | x > v  = go d (Just v) s  -- El actual es un posible predecesor

    findMin E' = Nothing
    findMin (N' E' v _) = Just v
    findMin (N' i _ _) = findMin i

    findMax E' = Nothing
    findMax (N' _ v E') = Just v
    findMax (N' _ _ d)  = findMax d


-- d 


elementosDesc :: Ord a => Heap a -> [a]
elementosDesc h = reverse (toSortedList h)
  where
    toSortedList EmptyH = []
    toSortedList h = findMin h : toSortedList (deleteMin h)
    
    findMin (NodeH _ x _ _) = x
    deleteMin (NodeH _ _ l r) = merge l r


-- e
esLeftist :: Ord a => Heap a -> Bool
esLeftist EmptyH = True
esLeftist (NodeH r x l d) =
    r == (rank d + 1) &&        -- Rango correcto
    rank l >= rank d &&         -- Propiedad leftist
    checkOrder x l &&           -- Orden de heap izq
    checkOrder x d &&           -- Orden de heap der
    esLeftist l && esLeftist d
  where
    rank EmptyH = 0
    rank (NodeH r _ _ _) = r
    checkOrder _ EmptyH = True
    checkOrder p (NodeH _ c _ _) = p <= c


-- f

import Data.List (nub)

eliminarDuplicados :: Ord a => Heap a -> Heap a
eliminarDuplicados h = fromList (nub (toList h))
  where
    -- 1. Convierte el Heap en una lista
    toList EmptyH = []
    toList (NodeH _ x l r) = x : toList l ++ toList r

    -- 2. Reconstruye el Heap desde la lista en tiempo lineal O(n)
    fromList [] = EmptyH
    fromList xs = mergePairs (map (\x -> NodeH 1 x EmptyH EmptyH) xs)

    -- 3. Fusión por pares para mantener la eficiencia
    mergePairs []  = EmptyH
    mergePairs [h] = h
    mergePairs hs  = mergePairs (proceso hs)

    proceso (h1:h2:rest) = merge h1 h2 : proceso rest
    proceso h            = h

    -- 4. Función Merge: el motor del Leftist Heap
    merge h1 EmptyH = h1
    merge EmptyH h2 = h2
    merge h1@(NodeH _ x1 l1 r1) h2@(NodeH _ x2 l2 r2)
        | x1 <= x2  = makeNode x1 l1 (merge r1 h2)
        | otherwise = makeNode x2 l2 (merge h1 r2)

    -- 5. Crea un nodo y asegura la propiedad izquierdista (rango izq >= rango der)
    makeNode x l r
        | rank l >= rank r = NodeH (rank r + 1) x l r
        | otherwise        = NodeH (rank l + 1) x r l

    -- 6. Obtiene el rango de un nodo de forma segura
    rank EmptyH = 0
    rank (NodeH r _ _ _) = r


-- g 
esRBT :: RBT a -> Bool
esRBT t = raizNegra t && noRojoSeguido t && soloUnaAlturaNegra t

raizNegra (Node B _ _ _) = True
raizNegra Empty = True
raizNegra _ = False

noRojoSeguido Empty = True
noRojoSeguido (Node R (Node R _ _ _) _ _) = False
noRojoSeguido (Node R _ _ (Node R _ _ _)) = False
noRojoSeguido (Node _ l _ r) = noRojoSeguido l && noRojoSeguido r

soloUnaAlturaNegra t = case checkHeight t of
                        Just _ -> True
                        Nothing -> False
  where
    checkHeight Empty = Just 1
    checkHeight (Node color l _ r) = do
        hI <- checkHeight l 
        hD <- checkHeight r
        if hI == hD 
            then Just (if color == B then hI + 1 else hI)
            else Nothing