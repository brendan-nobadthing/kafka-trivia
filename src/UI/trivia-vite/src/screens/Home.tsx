import { Button } from "@/components/ui/button"
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import {
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from "@/components/ui/tabs"
import { useGameStore } from '../store/GameStore'
 
export function Home() {

  const newGame = useGameStore(s => s.newGame);
  const checkNewGame = useGameStore(s => s.checkNewGame);
  return (
    <>
    <h1>Fast Trivia</h1>
    <Tabs defaultValue="join" className="w-[400px]">
      <TabsList className="grid w-full grid-cols-2">
        <TabsTrigger value="join">Join Game</TabsTrigger>
        <TabsTrigger value="new-game">New Game</TabsTrigger>
      </TabsList>
      <TabsContent value="join">
        <Card>
          <CardHeader>
            <CardTitle>Join Game</CardTitle>
            <CardDescription>
              Enter Game Code and your details to join. <br/>
              Email address is not shared - for gravitar pic only
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-2">
          <div className="space-y-1">
              <Label htmlFor="code">Game Code</Label>
              <Input id="code" defaultValue="" />
            </div>
            <div className="space-y-1">
              <Label htmlFor="name">Display Name</Label>
              <Input id="name" defaultValue="" />
            </div>
            <div className="space-y-1">
              <Label htmlFor="email">Email</Label>
              <Input id="email" type="email" defaultValue="" />
            </div>
          </CardContent>
          <CardFooter>
            <Button>Join Game</Button>
          </CardFooter>
        </Card>
      </TabsContent>
      <TabsContent value="new-game">
        <Card>
          <CardHeader>
            <CardTitle>New Game</CardTitle>
            <CardDescription>
             Give your game a name or code. Other users can use this to join your game
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-2">
            <div className="space-y-1">
              <Label htmlFor="new-game-name">Game Name</Label>
              <Input id="new-game-name"  /> {/* onChange={(e) => checkNewGame(e.target.value)} */}
            </div>      
          </CardContent>
          <CardFooter>
            <Button>Create Game</Button>
          </CardFooter>
        </Card>
      </TabsContent>
    </Tabs>
    </>
    
  )
}
